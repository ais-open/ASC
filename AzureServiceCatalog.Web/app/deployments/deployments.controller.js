(function () {
    'use strict';

    angular.module('ascApp').controller('DeploymentsCtrl', DeploymentsCtrl);

    DeploymentsCtrl.$inject = ['$state', 'initialData', 'ascApi'];

    /* @ngInject */
    function DeploymentsCtrl($state, initialData, ascApi) {
        /* jshint validthis: true */
        var vm = this;

        vm.deploymentChanged = deploymentChanged;
        vm.resourceGroups = [];
        vm.resourceGroupChanged = resourceGroupChanged;
        vm.subscriptions = initialData;

        activate();

        ////////////////

        function activate() {
            if (vm.subscriptions && vm.subscriptions.length > 0) {
                vm.selectedSubscription = vm.subscriptions[0];
                subscriptionChanged();
            }
        }

        function deploymentChanged() {
            $state.go('deployment', {
                subscriptionId: vm.selectedSubscription.id,
                correlationId: vm.selectedDeployment.properties.correlationId
            });
        }

        function resourceGroupChanged() {
            ascApi.getDeploymentList(vm.selectedResourceGroup.name, vm.selectedSubscription.id).then(function (data) {
                vm.deployments = data.deployments;
            });
        }

        function subscriptionChanged() {
            vm.resourceGroups = [];
            vm.resourceData = [];
            vm.selectedItem = null;
            ascApi.getResourceGroupsBySubscription(vm.selectedSubscription.rowKey).then(
                function (data) {
                    if (data !== undefined && data !== null) {
                        vm.resourceGroups = data.value;
                    }
                },
                function (error) {
                    alert(error);
                }
            );
        }
    }
})();