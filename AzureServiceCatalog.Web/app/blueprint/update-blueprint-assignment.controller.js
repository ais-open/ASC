(function () {
    'use strict';

    angular.module('ascApp').controller('UpdateBlueprintAssignmentCtrl', UpdateBlueprintAssignmentCtrl);
    UpdateBlueprintAssignmentCtrl.$inject = ['$state', 'initialData', 'ascApi', 'toastr'];

    /* @ngInject */
    function UpdateBlueprintAssignmentCtrl($state, initialData, ascApi, toastr) {
        /* jshint validthis: true */
        var vm = this;
        vm.lodash = _;
        vm.assignedBlueprint = null;
        vm.subscriptionId = "";
        vm.blueprintName = '';
        vm.assignmentName = '';
        vm.location = '';
        vm.targetScope = '';
        vm.blueprintVersions = [];
        vm.versionNames = [];
        vm.selectedVersion = '';
        vm.lockedAssigment = '';
        vm.parameters = [];
        vm.resourceGroups = [];
        vm.getBlueprintVersions = getBlueprintVersions;
        vm.onVersionChange = onVersionChange;
        vm.assign = assign;

        activate();

        function activate() {
            console.log(initialData);
            vm.assignedBlueprint = initialData;
            vm.assignmentName = initialData.name;

            //Getting subscription id from assigned blueprint id
            var tempArr = vm.assignedBlueprint.id.split('/');
            var subscriptionsIndex = tempArr.indexOf('subscriptions');
            vm.subscriptionId = tempArr[subscriptionsIndex + 1];

             //Getting blueprint name from blueprint id in properties
            var tempArr1 = vm.assignedBlueprint.properties.blueprintId.split('/');
            var blueprintsIndex = tempArr1.indexOf('blueprints');
            vm.blueprintName = tempArr1[blueprintsIndex + 1];

            var tempArr2 = vm.assignedBlueprint.properties.blueprintId.split('/');
            var versionsIndex = tempArr2.indexOf('versions');
            vm.selectedVersion = tempArr2[versionsIndex + 1];
            console.log(vm.assignedBlueprint.properties.scope.indexOf('subscriptions'));
            //.targetScope = vm.assignedBlueprint.properties.scope.contains('subscriptions') ? "Subscription": "Management Group";
            vm.location = vm.assignedBlueprint.location;
            vm.lockedAssigment = vm.assignedBlueprint.properties.locks.mode;
            getBlueprintVersions();
            //_.forIn(initialData.properties.parameters, function (value, key) {
            //    vm.parameters.push({ name: key, info: value, value: value.defaultValue });
            //});
            //console.log(vm.parameters.length);

            //_.forIn(initialData.properties.resourceGroups, function (value, key) {
            //    var resourceGroupInfo = value;
            //    var newRgObj = {
            //        "key": key,
            //        //"displayName": resourceGroupInfo.metadata.displayName,
            //        //"dependsOn": resourceGroupInfo.dependsOn
            //    };
            //    if (typeof resourceGroupInfo.location !== "undefined") {
            //        newRgObj.location = resourceGroupInfo.location;
            //    }
            //    if (typeof resourceGroupInfo.name !== "undefined") {
            //        newRgObj.name = resourceGroupInfo.name;
            //    }
            //    vm.resourceGroups.push(newRgObj);
           // });
        }

        function getBlueprintVersions() {
            ascApi.getBlueprintVersions(vm.subscriptionId, vm.blueprintName).then(function (data) {
                vm.blueprintVersions = data.value;
                if (vm.blueprintVersions.length > 0) {
                    vm.blueprintVersions.forEach(function (item) {
                        vm.versionNames.push(item.name);
                    });
                    var selectedBlueprintVersion = vm.blueprintVersions.find(i => i.name == vm.selectedVersion);
                    console.log(selectedBlueprintVersion);
                    getParametersAndResourceGroups(selectedBlueprintVersion);
                }
            });
        }

        function onVersionChange() {
            var selectedBlueprintVersion = vm.blueprintVersions.find(i => i.name == vm.selectedVersion);
            console.log(selectedBlueprintVersion);
            getParametersAndResourceGroups(selectedBlueprintVersion);
        }

        function getParametersAndResourceGroups(selectedBlueprintVersion) {
            var parameters = [];
            var resourceGroups = [];
            _.forIn(selectedBlueprintVersion.properties.parameters, function (value, key) {
                parameters.push({ name: key, info: value, value: value.defaultValue });
            });
            vm.parameters = parameters;
            _.forIn(selectedBlueprintVersion.properties.resourceGroups, function (value, key) {
                var resourceGroupInfo = value;
                var newRgObj = {
                    "key": key,
                    //"displayName": resourceGroupInfo.metadata.displayName,
                    //"dependsOn": resourceGroupInfo.dependsOn
                    "isLocationAvailable": false,
                    "isNameAvailable": false
                };
                if (typeof resourceGroupInfo.location !== "undefined") {
                    newRgObj.location = resourceGroupInfo.location;
                    newRgObj.isLocationAvailable = true;
                }
                if (typeof resourceGroupInfo.name !== "undefined") {
                    newRgObj.name = resourceGroupInfo.name;
                    newRgObj.isNameAvailable = true;
                }
                resourceGroups.push(newRgObj);
            });
            vm.resourceGroups = resourceGroups;
        }

        function assign() {
            if (vm.lockedAssigment === undefined) {
                vm.lockedAssigment = 'none';
            }
            
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
            console.log(blueprintAssignment);
            ascApi.assignBlueprint(vm.subscriptionId, vm.assignmentName, blueprintAssignment).then(function (data) {
                if (data.error) {
                    console.log('Error while assigning blueprint!', data);
                    toastr.error('Unexpected error while assigning.', 'Error');
                } else {
                    //vm.policy = data.policy;
                    toastr.success('Blueprint assigned successfully.', 'Success');
                    $state.go('manage-assigned-blueprint-list');
                }
            });
        }

    }
}
)();
