(function () {
    'use strict';

    angular.module('ascApp').controller('BlueprintsHomeCtrl', BlueprintsHomeCtrl);
    BlueprintsHomeCtrl.$inject = ['$uibModal','initialData', 'ascApi'];

    /* @ngInject */
    function BlueprintsHomeCtrl($uibModal, initialData, ascApi) {
        /* jshint validthis: true */
        var vm = this;
        vm.lodash = _;
        vm.blueprints = [];
        vm.selectedSubscription = null;
        vm.subscriptionId = "";
        vm.portalUrlForBlueprints = "";
        vm.extensionForUrl = "blade/Microsoft_Azure_Policy/BlueprintsMenuBlade/Blueprints";
        vm.subscriptions = initialData;
        vm.onChangeSelectedSubcription = onChangeSelectedSubcription;
        vm.getPortalUrlForBlueprints = vm.getPortalUrlForBlueprints;
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

        function onChangeSelectedSubcription(subscription) {
            vm.selectedSubscription = subscription;
            vm.subscriptionId = vm.selectedSubscription.rowKey;
            getBlueprints();
        }

        function getPortalUrlForBlueprints() {
            ascApi.getPortalUrl(vm.extensionForUrl, 'Common').then(function (data) {
                vm.portalUrlForBlueprints = data;
            });
        }

        function getBlueprints() {
            ascApi.getBlueprints(vm.subscriptionId).then(function (data) {
                vm.blueprints = data;
                getPortalUrlForBlueprints();
            });
        }

        function viewDetails(blueprint) {
            $uibModal.open({
                templateUrl: '/app/blueprint/view-blueprint-details-modal.html',
                controller: 'ViewBlueprintDetailsCtrl',
                controllerAs: 'vm',
                resolve: {
                    initialData: ['ascApi', function (ascApi) {
                        return blueprint;
                    }],
                    subscriptionId: function () {
                        return vm.subscriptionId;
                    },
                    subscriptionName: function () {
                        return vm.selectedSubscription.displayName;
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