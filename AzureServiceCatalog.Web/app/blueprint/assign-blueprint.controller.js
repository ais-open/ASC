(function () {
    'use strict';

    angular.module('ascApp').controller('AssignBlueprintCtrl', AssignBlueprintCtrl);
    AssignBlueprintCtrl.$inject = ['$uibModal', '$state', 'initialData', 'ascApi', 'toastr', 'dialogsService'];

    /* @ngInject */
    function AssignBlueprintCtrl($uibModal, $state, initialData, ascApi, toastr, dialogs) {
        /* jshint validthis: true */
        var vm = this;
        vm.subscriptionId = $state.params.subscriptionId;
        vm.subscriptionName = $state.params.subscriptionName;
        vm.lodash = _;
        vm.locations = [];
        vm.blueprintName = $state.params.blueprintName;
        vm.blueprintVersions = [];
        vm.versionNames = [];
        vm.assignmentName = "";
        vm.location = "";
        vm.selectedVersion = "";
        vm.lockAssignment = "";
        vm.managedIdentity = "SystemAssigned";
        vm.targetScope = "";
        vm.resourceGroups = [];
        vm.parameters = [];
        vm.extensionForUrl = "blade/Microsoft_AAD_IAM/UsersManagementMenuBlade/AllUsers";
        vm.extensionForBlueprints = "blade/Microsoft_Azure_Policy/BlueprintsMenuBlade/Blueprints";
        vm.portalUrlForUsers = "";
        vm.portalUrlForBlueprints = "";
        vm.getPortalUrlForUsers = getPortalUrlForUsers;
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
                    vm.assignmentName = 'Assignment-' + vm.blueprintName;
                    vm.targetScope = vm.blueprintVersions[0].scope;
                    vm.blueprintVersions.forEach(function (item) {
                        versionNames.push(item.name);
                    });
                    vm.versionNames = versionNames;
                    vm.selectedVersion = vm.versionNames[vm.versionNames.length - 1];
                    var selectedBlueprintVersionIndex = vm.versionNames.findIndex(i => i == vm.selectedVersion);
                    vm.selectedBlueprintVersion = vm.blueprintVersions[selectedBlueprintVersionIndex];
                    getParameters(vm.selectedBlueprintVersion);
                    getResourceGroups(vm.selectedBlueprintVersion);
                    getPortalUrlForUsers();
                    getLocations();
                } else {
                    getPortalUrlForBlueprints();
                }
            }
        }

        function getParameters(selectedBlueprintVersion) {
            var parameters = [];
            _.forIn(selectedBlueprintVersion.parameters, function (param, key) {
                var newParameter = {
                    "name": key,
                    "value": "",
                    "displayName": "",
                    "strongType": "",
                    "description": "",
                    "type": param.type,
                    "allowedValues": []
                };
                if (typeof param.metadata !== "undefined") {
                    if (typeof param.metadata.displayName !== "undefined") {
                        newParameter.displayName = param.metadata.displayName;
                    }
                    if (typeof param.metadata.strongType !== "undefined") {
                        newParameter.strongType = param.metadata.strongType;
                    }
                    if (typeof param.metadata.description !== "undefined") {
                        newParameter.description = param.metadata.description;
                    }
                }
                if (typeof param.allowedValues !== "undefined") {
                    newParameter.allowedValues = param.allowedValues;
                }
                if (typeof param.defaultValue !== "undefined") {
                    newParameter.value = param.defaultValue;
                }
                parameters.push(newParameter);
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

        function getPortalUrlForBlueprints() {
            ascApi.getPortalUrl(vm.extensionForBlueprints, 'Common').then(function (data) {
                vm.portalUrlForBlueprints = data;
            });
        }

        function getPortalUrlForUsers() {
            ascApi.getPortalUrl(vm.extensionForUrl, 'Common').then(function (data) {
                vm.portalUrlForUsers = data;
            });
        }

        function getLocations() {
            ascApi.getStorageProvider(vm.subscriptionId).then(function (data) {
                var storageAccounts = _.find(data.resourceTypes, { resourceType: 'storageAccounts' });
                vm.locations = storageAccounts.locations;
            });
        }

        function onVersionChange() {
            var selectedBlueprintVersionIndex = vm.versionNames.findIndex(i => i == vm.selectedVersion);
            vm.selectedBlueprintVersion = vm.blueprintVersions[selectedBlueprintVersionIndex];
            getParameters(vm.selectedBlueprintVersion);
            getResourceGroups(vm.selectedBlueprintVersion);
        }

        function assignBlueprint() {
            var isErrorNull = true;
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
            var parameters = {};
            var resourceGroups = {};
            _.forIn(vm.parameters, function (param) {
                var parameterValue = param.value;
                if (param.type === "array") {
                    try {
                        parameterValue = parameterValue.split(',');
                    }
                    catch (exc) {
                        isErrorNull = false;
                        var heading = 'Invalid Parameter Value (' + param.name + ')';
                        var msg = 'Parameter value should be comma seperated string values.';
                        toastr.warning(msg, heading);
                    }
                } else if (param.type === "int") {
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
            if (isErrorNull) {
                ascApi.assignBlueprint(vm.subscriptionId, vm.assignmentName, blueprintAssignment).then(function (data) {
                    if (data.error) {
                        console.log('Error while assigning blueprint!', data);
                        toastr.error('Unexpected error while assigning.', 'Error');
                    } else {
                        toastr.success('Blueprint assigned successfully.', 'Success');
                        $state.go('blueprint-assignments', { subscriptionId: vm.subscriptionId, subscriptionName: vm.subscriptionName });
                    }
                });
            }
        }

        function assign() {
            ascApi.getBlueprintAssignment(vm.subscriptionId, vm.assignmentName).then(function (data) {
                if (data != null) {
                    var msg = 'A resource already exists with the same name in this scope. Please choose a different name.';
                    toastr.warning(msg, 'Invalid Name');
                    return;
                } else {
                    dialogs.confirm('Are you sure you want to assign the blueprint?', 'Assign Blueprint', ['Yes', 'No']).then(function () {
                        assignBlueprint();
                    });
                }
            });
        }
    }
})();