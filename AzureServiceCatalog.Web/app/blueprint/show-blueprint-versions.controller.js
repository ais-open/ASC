(function () {
    'use strict';

    angular.module('ascApp').controller('ShowBlueprintVersionsCtrl', ShowBlueprintVersionsCtrl);
    ShowBlueprintVersionsCtrl.$inject = ['$stateParams', '$timeout', '$state', 'initialData', 'ascApi', 'toastr', 'dialogsService'];

    /* @ngInject */
    function ShowBlueprintVersionsCtrl($stateParams, $timeout, $state, initialData, ascApi, toastr, dialogs) {
        /* jshint validthis: true */
        var vm = this;
        vm.blueprintVersions = [];
        vm.blueprintName = "";
        vm.blueprintDescription = "";
        vm.versionNames = [];
        vm.latestVersion = "";
        vm.selectedSubscription = null;
        vm.assign = assign;

        activate();

        function activate() {
            if (initialData) {
                vm.blueprintVersions = initialData.value;
                var versionNames = [];
                if (vm.blueprintVersions && vm.blueprintVersions.length > 0) {
                    vm.blueprintName = vm.blueprintVersions[0].properties.blueprintName;
                    vm.blueprintDescription = vm.blueprintVersions[0].properties.description;
                    console.log(vm.blueprintVersions);
                    vm.blueprintVersions.forEach(function (item) {
                        console.log(item)
                        versionNames.push(item.name);
                    });
                    console.log(versionNames);
                    vm.versionNames = versionNames;
                    vm.latestVersion = vm.versionNames[vm.versionNames.length - 1];
                }
            }
        }

        function assign() {
        }
    }
})();