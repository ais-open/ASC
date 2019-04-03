(function () {
    'use strict';

    angular.module('ascApp').controller('PolicyListCtrl', PolicyListCtrl);
    PolicyListCtrl.$inject = ['initialData', 'ascApi'];

    /* @ngInject */
    function PolicyListCtrl(initialData, ascApi) {
        /* jshint validthis: true */
        var vm = this;
        vm.policies = [];
        vm.selectedSubscription = null;
        vm.getPolicyDefName = getPolicyDefName;
        vm.subscriptions = initialData;
        vm.subscriptionChanged = subscriptionChanged;

        activate();

        function activate() {
            if (vm.subscriptions && vm.subscriptions.length > 0) {
                vm.selectedSubscription = vm.subscriptions[0];
                subscriptionChanged();
            }
        }

        function getPolicyDefName(policy) {
            var name = "";
            if (policy.properties["displayName"] === undefined) {
                name = policy.name;
            }
            else {
                name = policy.properties["displayName"];
            }

            return name;
        }

        function subscriptionChanged() {
            ascApi.getPolicies(vm.selectedSubscription.rowKey).then(function (data) {
                vm.policies = _.filter(data, function (item) {
                    return item.policy.properties.policyType === 'Custom';
                });
            });
        }
    }
})();