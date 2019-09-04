(function () {
    'use strict';

    angular.module('ascApp').controller('AssignBlueprintCtrl', AssignBlueprintCtrl);
    AssignBlueprintCtrl.$inject = ['$state', 'initialData', 'ascApi', 'toastr'];

    /* @ngInject */
    function AssignBlueprintCtrl($state, initialData, ascApi, toastr) {
        /* jshint validthis: true */
        var vm = this;
        vm.subscriptionId = "";
        vm.lodash = _;
        vm.locations = [];
        vm.blueprintName = "";
        vm.blueprintVersions = [];
        vm.versionNames = [];
        vm.assignmentName = "";
        vm.location = "";
        vm.selectedVersion = "";
        vm.lockAssignment = "none";
        vm.managedIdentity = "SystemAssigned";
        vm.userAssignedIdentities = [];
        vm.selectedUserIdentity = "";
        vm.targetScope = "";
        vm.resourceGroups = [];
        vm.parameters = [];
        vm.getLocations = getLocations;
        vm.getParameters = getParameters;
        vm.getResourceGroups = getResourceGroups;
        vm.onVersionChange = onVersionChange;
        vm.assign = assign;

        activate();

        function activate() {
            if (initialData) {
                vm.blueprintVersions = initialData;
                var versionNames = [];
                if (vm.blueprintVersions && vm.blueprintVersions.length > 0) {
                    vm.blueprintName = vm.blueprintVersions[0].blueprintName;
                    vm.assignmentName = 'Assignment-' + vm.blueprintName;
                    vm.targetScope = vm.blueprintVersions[0].scope;
                    var tempArr = vm.blueprintVersions[0].id.split('/');
                    var subscriptionsIndex = tempArr.indexOf('subscriptions');
                    vm.subscriptionId = tempArr[subscriptionsIndex + 1];
                    vm.blueprintVersions.forEach(function (item) {
                        versionNames.push(item.name);
                    });
                    vm.versionNames = versionNames;
                    vm.selectedVersion = vm.versionNames[vm.versionNames.length - 1];
                    var selectedBlueprintVersionIndex = vm.versionNames.findIndex(i => i == vm.selectedVersion);
                    vm.selectedBlueprintVersion = vm.blueprintVersions[selectedBlueprintVersionIndex];
                    getParameters(vm.selectedBlueprintVersion);
                    getResourceGroups(vm.selectedBlueprintVersion);
                }
            }
            getLocations();
            getUserAssignedIdentities();
        }

        function getParameters(selectedBlueprintVersion) {
            var parameters = [];
            _.forIn(selectedBlueprintVersion.parameters, function (value, key) {
                parameters.push({ name: key, info: value, value: value.defaultValue });
            });
            vm.parameters = parameters;
        }

        function getResourceGroups(selectedBlueprintVersion) {
            var resourceGroups = [];
            _.forIn(selectedBlueprintVersion.resourceGroups, function (rg, key) {
                var newRg = {
                    "key": key,
                    "dependsOn": rg.dependsOn,
                    "isLocationAvailable": false,
                    "isNameAvailable": false
                };
                if (typeof rg.metadata !== "undefined") {
                    newRg.displayName = rg.metadata.displayName;
                }
                if (typeof rg.location !== "undefined") {
                    newRg.location = rg.location;
                    newRg.isLocationAvailable = true;
                }
                if (typeof rg.name !== "undefined") {
                    newRg.name = rg.name;
                    newRg.isNameAvailable = true;
                }
                resourceGroups.push(newRg);
            });
            vm.resourceGroups = resourceGroups;
        }

        function getLocations() {
            ascApi.getStorageProvider(vm.subscriptionId).then(function (data) {
                var storageAccounts = _.find(data.resourceTypes, { resourceType: 'storageAccounts' });
                vm.locations = storageAccounts.locations;
            });
        }

        function getUserAssignedIdentities() {
            ascApi.getUserAssignedIdentities(vm.subscriptionId).then(function (data) {
                vm.userAssignedIdentities = data;
            });
        }

        function onVersionChange() {
            var selectedBlueprintVersionIndex = vm.versionNames.findIndex(i => i == vm.selectedVersion);
            vm.selectedBlueprintVersion = vm.blueprintVersions[selectedBlueprintVersionIndex];
            getParameters(vm.selectedBlueprintVersion);
            getResourceGroups(vm.selectedBlueprintVersion);
        }

        function assign() {
            ascApi.getAssignedBlueprint(vm.subscriptionId, vm.assignmentName).then(function (data) {
                if (typeof data.error === "undefined") {
                    var msg = 'A resource already exists with the same name in this scope. Please choose a different name.';
                    toastr.warning(msg, 'Invalid Name');
                    return;
                } else {
                    var blueprintAssignment = {
                        "identity": {
                            "type": vm.managedIdentity
                        },
                        "location": vm.location,
                        "properties": {
                            "blueprintId": vm.selectedBlueprintVersion.id,
                            "locks": {
                                "mode": vm.lockAssignment
                            }
                        }
                    };
                    if (vm.managedIdentity == "UserAssigned") {
                        blueprintAssignment.identity["userAssignedIdentities"] = vm.selectedUserIdentity.id;
                    }
                    var parameters = {};
                    var resourceGroups = {};
                    _.forIn(vm.parameters, function (param) {
                        var parameterValue = param.value;
                        if (param.info.type === "array" || param.info.type === "object") {
                            parameterValue = JSON.parse(parameterValue);
                        } else if (param.info.type === "int") {
                            parameterValue = parseInt(parameterValue, 10);
                        }
                        parameters[`${param.name}`] = {
                            "value": parameterValue
                        }
                    });
                    blueprintAssignment.properties['parameters'] = parameters;
                    _.forIn(vm.resourceGroups, function (rg) {
                        if (!rg.isNameAvailable || !rg.isLocationAvailable) {
                            resourceGroups[`${rg.key}`] = {};
                            //If name is not assigned in blueprint definition, add name property
                            if (!rg.isNameAvailable) {
                                resourceGroups[`${rg.key}`]["name"] = rg.name;
                            }
                            //If location is not assigned in blueprint definition, add location property
                            if (!rg.isLocationAvailable) {
                                resourceGroups[`${rg.key}`]["location"] = rg.location;
                            }
                        }
                    });
                    blueprintAssignment.properties['resourceGroups'] = resourceGroups;
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
})();