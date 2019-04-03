(function () {
    'use strict';

    angular.module('ascApp').controller('ManageHostSubscriptionCtrl', ManageHostSubscriptionCtrl);
    ManageHostSubscriptionCtrl.$inject = ['initialData', 'ascApi', 'toastr'];

    /* @ngInject */
    function ManageHostSubscriptionCtrl(initialData, ascApi, toastr) {
        /* jshint validthis: true */
        var vm = this;
        vm.divRGVisible = false;
        vm.save = save;
        vm.hostSubscriptionChanged = hostSubscriptionChanged;
        vm.subscriptions = initialData;
        vm.toggleDivRGVisible = toggleDivRGVisible;

        activate();

        function activate() {
            vm.selectedHost = _.find(vm.subscriptions, { 'isConnected': true });
            hostSubscriptionChanged();
        }

        function hostSubscriptionChanged() {
            vm.resourceGroups = [];
            vm.selectedResourceGroup = null;
            ascApi.getUserResourceGroupsBySubscription(vm.selectedHost.id).then(
                function (data) {
                    if (data) {
                        vm.resourceGroups = data.value;
                    }
                },
                function (error) {
                    toastr.error(error, 'Error');
                }
            );
            toggleDivRGVisible(vm.selectedHost.isConnected);
        }

        function toggleDivRGVisible(isConnected) {
            vm.divRGVisible = !isConnected;
        }

        function save() {
            _.forEach(vm.subscriptions, function (item) {
                item.isConnected = (vm.selectedHost === item);
                if (vm.selectedHost === item) {
                    item.isConnected = true;
                } else {
                    item.isConnected = false;
                }
            });

            var subscriptionsResource = {
                subscriptions: vm.subscriptions,
                resourceGroup: vm.selectedResourceGroup.name,
                location: vm.selectedResourceGroup.location
            };

            ascApi.saveSubscriptions(subscriptionsResource).then(function (data) {
                toastr.success('The Subscriptions were saved successfully.', 'Save Successful');
            });
        }
    }
})();