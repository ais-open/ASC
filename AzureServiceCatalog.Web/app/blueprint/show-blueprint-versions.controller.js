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
        vm.selectedVersion = "";
        vm.subscriptionId = "";
        vm.onVersionChange = onVersionChange;
        vm.assign = assign;
        vm.showDiv = true;

        activate();

        function activate() {
            if (initialData) {
                vm.blueprintVersions = initialData.value;
                var versionNames = [];
                if (vm.blueprintVersions && vm.blueprintVersions.length > 0) {
                    vm.blueprintName = vm.blueprintVersions[0].properties.blueprintName;
                    var tempArr = vm.blueprintVersions[0].id.split('/');
                    var subscriptionsIndex = tempArr.indexOf('subscriptions');
                    vm.subscriptionId = tempArr[subscriptionsIndex + 1];
                    vm.blueprintVersions.forEach(function (item) {
                        versionNames.push(item.name);
                    });
                    vm.versionNames = versionNames;
                    vm.selectedVersion = vm.versionNames[vm.versionNames.length - 1];
                    vm.blueprintDescription = vm.blueprintVersions[vm.versionNames.length - 1].properties.description;
                } else {
                    vm.showDiv = false;
                }
            }
        }

        function onVersionChange() {
            var selectedBlueprintVersionIndex = vm.versionNames.findIndex(i => i == vm.selectedVersion);
            vm.blueprintDescription = vm.blueprintVersions[selectedBlueprintVersionIndex].properties.description;
        }

        function assign() {
        }
    }
})();