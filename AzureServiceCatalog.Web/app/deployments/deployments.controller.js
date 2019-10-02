(function () {
    'use strict';

    angular.module('ascApp').controller('DeploymentsCtrl', DeploymentsCtrl);

    DeploymentsCtrl.$inject = ['$state', 'ascApi', 'appStorage'];

    /* @ngInject */
    function DeploymentsCtrl($state, ascApi, appStorage) {
        /* jshint validthis: true */
        var vm = this;

        vm.deploymentChanged = deploymentChanged;
        vm.resourceGroups = [];
        vm.resourceGroupChanged = resourceGroupChanged;
        vm.subscriptions = null;
        vm.getEnrolledSubscription = getEnrolledSubscription;

        activate();

        ////////////////

        function activate() {
            vm.subscriptions = getEnrolledSubscription();

            if (vm.subscriptions && vm.subscriptions.length > 0) {
                vm.selectedSubscription = vm.subscriptions[0];
                subscriptionChanged();
            }
        }

        function getEnrolledSubscription() {
            vm.subscriptions = getEnrolledSubscription();
            var enrolledSubscription = appStorage.getEnrolledSubscription();

            if (enrolledSubscription === null) {
                enrolledSubscription = [];
                ascApi.getEnrolledSubscriptions().then(function (data) {
                    for (var i = 0, length = data.length; i < length; i++) {
                        enrolledSubscription.push(data[i]);
                    }
                    appStorage.setEnrolledSubscription(JSON.stringify(enrolledSubscription));
                });
            } else {
                enrolledSubscription = JSON.parse(enrolledSubscription);
            }
            return enrolledSubscription;
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