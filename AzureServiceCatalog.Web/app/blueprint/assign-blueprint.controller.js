(function () {
    'use strict';

    angular.module('ascApp').controller('AssignBlueprintCtrl', AssignBlueprintCtrl);
    AssignBlueprintCtrl.$inject = ['$state', 'initialData', 'ascApi', 'toastr'];

    /* @ngInject */
    function AssignBlueprintCtrl($state, initialData, ascApi, toastr) {
        /* jshint validthis: true */
        var vm = this;
        vm.blueprintVersion = null;
        vm.subscriptionId = "";
        vm.lodash = _;
        vm.parameters = [];
        vm.resourceGroups = [];
        vm.blueprintDefinitionVersion = initialData.name;
        vm.blueprintName = initialData.properties.blueprintName;
        vm.assignmentName = 'Assignment-' + vm.blueprintName;
        vm.targetScope = initialData.properties.targetScope;
        vm.location = '';
        vm.lockedAssigment = 'none';
        vm.locations = [];
        vm.hostSubscriptionChanged = hostSubscriptionChanged;
        vm.assign = assign;

        activate();

        function activate() {
            vm.blueprintVersion = initialData;
            //console.log(initialData);
            var tempArr = vm.blueprintVersion.id.split('/');
            var subscriptionsIndex = tempArr.indexOf('subscriptions');
            vm.subscriptionId = tempArr[subscriptionsIndex + 1];
            hostSubscriptionChanged();
            _.forIn(initialData.properties.parameters, function (value, key) {
                vm.parameters.push({ name: key, info: value, value: value.defaultValue });
            });

            _.forIn(initialData.properties.resourceGroups, function (value, key) {
                var resourceGroupInfo = value;
                var newRgObj = {
                    "key": key,
                    "dependsOn": resourceGroupInfo.dependsOn,
                    "isLocationAvailable": false,
                    "isNameAvailable": false
                };
                if (typeof resourceGroupInfo.metadata !== "undefined") {
                    newRgObj.displayName = resourceGroupInfo.metadata.displayName;
                }
                if (typeof resourceGroupInfo.location !== "undefined") {
                    newRgObj.location = resourceGroupInfo.location;
                    newRgObj.isLocationAvailable = true;
                }
                if (typeof resourceGroupInfo.name !== "undefined") {
                    newRgObj.name = resourceGroupInfo.name;
                    newRgObj.isNameAvailable = true;
                }
                vm.resourceGroups.push(newRgObj);
            });
        }

        function hostSubscriptionChanged() {
            ascApi.getStorageProvider(vm.subscriptionId).then(function (data) {
                var storageAccounts = _.find(data.resourceTypes, { resourceType: 'storageAccounts' });
                vm.locations = storageAccounts.locations;
            });
        }

        function assign() {
            //checking if assignment name already exists or not
            ascApi.getAssignedBlueprint(vm.subscriptionId, vm.assignmentName).then(function (data) {
                console.log(data);
                if (typeof data.error === "undefined") {
                  var msg = 'A resource already exists with this name in this scope. Please choose a different name.';
                  toastr.warning(msg, 'Invalid Name');
                  return;
                } else {
                    var blueprintAssignment = {
                        "identity": {
                            "type": "SystemAssigned"
                        },
                        "location": vm.location,
                        "properties": {
                            "blueprintId": vm.blueprintVersion.id,
                            "locks": {
                                "mode": vm.lockedAssigment
                            }
                        }
                    };
                    var parameters = {};
                    var resourceGroups = {};
                    _.forIn(vm.parameters, function (objValue, key) {
                        var parameterValue = objValue.value;
                        if (objValue.info.type === "array" || objValue.info.type === "object") {
                            parameterValue = JSON.parse(parameterValue);
                        } else if (objValue.info.type === "int") {
                            parameterValue = parseInt(parameterValue, 10);
                        }
                        parameters[`${objValue.name}`] = {
                            "value": parameterValue
                        }
                    });
                    blueprintAssignment.properties['parameters'] = parameters;
                    _.forIn(initialData.properties.resourceGroups, function (objValue, key) {
                        if (typeof objValue.name == "undefined" || typeof objValue.location == "undefined") {
                            resourceGroups[`${key}`] = {};
                            var matchingRg = vm.resourceGroups.find(i => i.key === key);
                            //If name is not assigned in blueprint definition, add name property
                            if (typeof objValue.name == "undefined") {
                                resourceGroups[`${key}`]["name"] = matchingRg.name;
                            }
                            //If location is not assigned in blueprint definition, add location property
                            if (typeof objValue.location == "undefined") {
                                resourceGroups[`${key}`]["location"] = matchingRg.location;
                            }
                        }
                    });
                    blueprintAssignment.properties['resourceGroups'] = resourceGroups;
                    //console.log(blueprintAssignment);
                    ascApi.assignBlueprint(vm.subscriptionId, vm.assignmentName, blueprintAssignment).then(function (data) {
                        if (data.error) {
                            console.log('Error while assigning blueprint!', data);
                            toastr.error('Unexpected error while assigning.', 'Error');
                        } else {
                            toastr.success('Blueprint assigned successfully.', 'Success');
                            $state.go('manage-assigned-blueprint-list');
                        }
                    });
                }
            });
        }

    }
}
)();
