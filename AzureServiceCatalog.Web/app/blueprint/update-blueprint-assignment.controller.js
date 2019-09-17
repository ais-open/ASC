(function () {
    'use strict';

    angular.module('ascApp').controller('UpdateBlueprintAssignmentCtrl', UpdateBlueprintAssignmentCtrl);
    UpdateBlueprintAssignmentCtrl.$inject = ['$uibModal', '$state', 'initialData', 'ascApi', 'toastr'];

    /* @ngInject */
    function UpdateBlueprintAssignmentCtrl($uibModal, $state, initialData, ascApi, toastr) {
        /* jshint validthis: true */
        var vm = this;
        vm.subscriptionId = $state.params.subscriptionId;
        vm.subscriptionName = $state.params.subscriptionName;
        vm.lodash = _;
        vm.blueprintName = "";
        vm.blueprintAssignment = null;
        vm.blueprintVersions = [];
        vm.versionNames = [];
        vm.selectedVersion = "";
        vm.lockAssignment = "";
        vm.managedIdentity = "";
        vm.userAssignedIdentities = [];
        vm.selectedUserIdentity = "";
        vm.targetScope = "";
        vm.resourceGroups = [];
        vm.parameters = [];
        vm.extensionForUrl = "blade/Microsoft_AAD_IAM/UsersManagementMenuBlade/AllUsers";
        vm.portalUrlForUsers = "";
        vm.getPortalUrlForUsers = getPortalUrlForUsers;
        vm.getBlueprintVersions = getBlueprintVersions;
        vm.getParameters = getParameters;
        vm.getResourceGroups = getResourceGroups;
        vm.onVersionChange = onVersionChange;
        vm.selectFromUsersList = selectFromUsersList;
        vm.update = update;

        activate();

        function activate() {
            if (initialData) {
                vm.blueprintAssignment = initialData;
                var tempArr = vm.blueprintAssignment.id.split('/');
                var subscriptionsIndex = tempArr.indexOf('subscriptions');
                vm.subscriptionId = tempArr[subscriptionsIndex + 1];
                vm.lockAssignment = vm.blueprintAssignment.lockMode;
                vm.managedIdentity = vm.blueprintAssignment.managedIdentity.charAt(0).toUpperCase() + vm.blueprintAssignment.managedIdentity.slice(1)
                vm.selectedVersion = vm.blueprintAssignment.blueprintVersion;
                vm.targetScope = vm.blueprintAssignment.scope.includes('subscriptions') ? "Subscription": "Management Group";
            }
            getPortalUrlForUsers(); 
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
            if (vm.blueprintAssignment.blueprintVersion == selectedBlueprintVersion.name) {
                _.forIn(vm.blueprintAssignment.parameters, function (data, key) {
                    var matchingParameterIndex = vm.parameters.findIndex(i => i.name == key);
                    if (matchingParameterIndex >= 0) {
                        var matchingParameter = vm.parameters[matchingParameterIndex];
                        if (matchingParameter.value == "") {
                            var parameterValue = data.value;
                            if (typeof parameterValue == 'object') {
                                parameterValue = JSON.stringify(parameterValue);
                            }
                            matchingParameter.value = parameterValue;
                        }
                        vm.parameters.splice(matchingParameterIndex, 1, matchingParameter);
                    }
                });
            }
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
            if (vm.blueprintAssignment.blueprintVersion == selectedBlueprintVersion.name) {
                _.forIn(vm.blueprintAssignment.resourceGroups, function (data, key) {
                    var matchingResourceGroupIndex = vm.resourceGroups.findIndex(i => i.key == key);
                    if (matchingResourceGroupIndex >= 0) {
                        var matchingResourceGroup = vm.resourceGroups[matchingResourceGroupIndex];
                        if (typeof data.name !== "undefined") {
                            matchingResourceGroup['name'] = data.name;
                        }
                        if (typeof data.location !== "undefined") {
                            matchingResourceGroup['location'] = data.location;
                        }
                        vm.resourceGroups.splice(matchingResourceGroupIndex, 1, matchingResourceGroup);
                    }
                });
            }
        }

        function getPortalUrlForUsers() {
            ascApi.getPortalUrl(vm.extensionForUrl, 'Common').then(function (data) {
                vm.portalUrlForUsers = data;
                getBlueprintVersions(vm.blueprintAssignment.blueprintName);
            });
        }

        function getBlueprintVersions(blueprintName) {
            ascApi.getBlueprintVersions(vm.subscriptionId, blueprintName).then(function (data) {
                vm.blueprintVersions = data;
                if (vm.blueprintVersions.length > 0) {
                    vm.blueprintVersions.forEach(function (item) {
                        vm.versionNames.push(item.name);
                    });
                    vm.selectedBlueprintVersion = vm.blueprintVersions.find(i => i.name == vm.selectedVersion);
                    getParameters(vm.selectedBlueprintVersion);
                    getResourceGroups(vm.selectedBlueprintVersion);
                }
                getUserAssignedIdentities();
            });
        }

        function getUserAssignedIdentities() {
            ascApi.getUserAssignedIdentities(vm.subscriptionId).then(function (data) {
                vm.userAssignedIdentities = data;
                if (vm.managedIdentity == "UserAssigned") {
                    for (var key in vm.blueprintAssignment.userAssignedIdentities) {
                        var keyArr = key.split('/');
                        var identityIndex = keyArr.indexOf('userAssignedIdentities');
                        vm.selectedUserIdentity = keyArr[identityIndex + 1];
                        vm.selectedUserIdentity = vm.userAssignedIdentities.find(i => i.name == keyArr[identityIndex + 1])
                    }
                }
            });
            document.getElementById('BlueprintVersion').focus();
        }

        function onVersionChange() {
            var selectedBlueprintVersionIndex = vm.versionNames.findIndex(i => i == vm.selectedVersion);
            vm.selectedBlueprintVersion = vm.blueprintVersions[selectedBlueprintVersionIndex];
            getParameters(vm.selectedBlueprintVersion);
            getResourceGroups(vm.selectedBlueprintVersion);
        }

        function selectFromUsersList(parameter) {
            var rgDialog = $uibModal.open({
                templateUrl: '/app/blueprint/users-list-modal.html',
                controller: 'UsersListCtrl',
                controllerAs: 'vm',
                resolve: {
                    initialData: ['usersListInitialDataService', function (usersListInitialDataService) {
                        return parameter;
                    }]
                },
                size: 'lg'
            });
            rgDialog.result.then(function (selectedUsers) {
                
            });
        }

        function update() {
            var isErrorNull = true;
            var blueprintAssignment = {
                "identity": {
                    "type": vm.managedIdentity
                },
                "location": vm.blueprintAssignment.location,
                "properties": {
                    "blueprintId": vm.selectedBlueprintVersion.id,
                    "locks": {
                        "mode": vm.lockAssignment
                    }
                }
            };
            if (vm.managedIdentity == "UserAssigned") {
                var selectedIdentity = vm.selectedUserIdentity.id;
                blueprintAssignment.identity["userAssignedIdentities"] = {
                    [`${selectedIdentity}`]: {}
                };
            }
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
                ascApi.assignBlueprint(vm.subscriptionId, vm.blueprintAssignment.name, blueprintAssignment).then(function (data) {
                    if (data.error) {
                        console.log('Error while assigning blueprint!', data);
                        toastr.error('Unexpected error while assigning.', 'Error');
                    } else {
                        toastr.success('Blueprint assigned successfully.', 'Success');
                        //$state.go('manage-assigned-blueprint-list');
                    }
                });
            }
        }
    }
})();