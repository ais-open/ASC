(function () {
    'use strict';

    angular.module('ascApp').controller('BlueprintsHomeCtrl', BlueprintsHomeCtrl);
    BlueprintsHomeCtrl.$inject = ['$uibModal','initialData', 'ascApi'];

    /* @ngInject */
    function BlueprintsHomeCtrl($uibModal, initialData, ascApi) {
        /* jshint validthis: true */
        var vm = this;
        vm.blueprints = [];
        vm.selectedSubscription = null;
        vm.subscriptionId = "";
        vm.portalUrl = "";
        vm.subscriptions = initialData;
        vm.getBlueprints = getBlueprints;
        vm.viewDetails = viewDetails;
        vm.getLastModifiedDate = getLastModifiedDate;
        activate();

        function activate() {
            if (vm.subscriptions && vm.subscriptions.length > 0) {
                vm.selectedSubscription = vm.subscriptions[0];
                vm.subscriptionId = vm.selectedSubscription.rowKey;
                getBlueprints();
            }
        }

        function getBlueprints() {
            ascApi.getBlueprints(vm.subscriptionId).then(function (data) {
                vm.blueprints = data;
                vm.portalUrl = vm.blueprints[0].portalUrl;
            });
        }

        function viewDetails(blueprint) {
            $uibModal.open({
                templateUrl: '/app/blueprint/view-blueprint-details-modal.html',
                controller: 'ViewBlueprintDetailsCtrl',
                controllerAs: 'vm',
                resolve: {
                    initialData: ['viewBlueprintDetailsInitialDataService', function (viewBlueprintDetailsInitialDataService) {
                        return viewBlueprintDetailsInitialDataService.getData(blueprint);
                    }],
                    subscriptionId: function () {
                        return vm.subscriptionId;
                    }
                },
                size: 'lg'
            });

        }

        function getLastModifiedDate(lastModifiedDate) {
            var newDate = new Date(lastModifiedDate);
            var updatedLastModifiedDate = newDate.toLocaleDateString();
            return updatedLastModifiedDate;
        }
    }
})();