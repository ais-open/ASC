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
        //vm.parameters = [];
        //vm.resourceGroups = [];
        //vm.targetScope = initialData.properties.targetScope;
        //vm.lockedAssigment = 'none';
        //vm.locations = [];
        vm.blueprintName = "";
        vm.blueprintVersions = [];
        vm.versionNames = [];
        vm.assignmentName = "";
        vm.location = "";
        vm.selectedVersion = "";
        vm.lockAssigment = 'none';
        vm.targetScope = "";
        vm.resourceGroups = [];
        vm.parameters = [];
        vm.getLocations = getLocations;
        vm.getParameter = getParameter;
        vm.getResourceGroup = getResourceGroup;
        vm.assign = assign;

        activate();

        function activate() {
            console.log(initialData);
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
                    _.forIn(vm.selectedBlueprintVersion.parameters, function (value, key) {
                        vm.parameters.push({ name: key, info: value, value: value.defaultValue });
                    });

                    _.forIn(vm.selectedBlueprintVersion.resourceGroups, function (rg, key) {
                        var newRg = getResourceGroup(rg, key);
                        vm.resourceGroups.push(newRg);
                    });
                }
            }
            getLocations();
        }

        function getParameter() {

        }

        function getResourceGroup(rg, key) {
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
            return newRg;
        }

        function getLocations() {
            ascApi.getStorageProvider(vm.subscriptionId).then(function (data) {
                var storageAccounts = _.find(data.resourceTypes, { resourceType: 'storageAccounts' });
                vm.locations = storageAccounts.locations;
            });
        }

        function assign() {

        }
    }
})();