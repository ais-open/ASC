(function () {
    'use strict';

    angular.module('ascApp').controller('PolicyAssignmentListCtrl', PolicyAssignmentListCtrl);
    PolicyAssignmentListCtrl.$inject = ['initialData', 'ascApi'];

    /* @ngInject */
    function PolicyAssignmentListCtrl(initialData, ascApi) {
        /* jshint validthis: true */
        var vm = this;
        vm.getPolicyDefName = getPolicyDefName;
        vm.policyAssignments = [];
        vm.selectedSubscription = null;
        vm.subscriptions = initialData;
        vm.subscriptionChanged = subscriptionChanged;

        activate();

        function activate() {
            if (vm.subscriptions && vm.subscriptions.length > 0) {
                vm.selectedSubscription = vm.subscriptions[0];
                subscriptionChanged();
            }
        }

        function getPolicyDefName(policyDefId) {
            var lastSlash = policyDefId.lastIndexOf('/');
            return policyDefId.substring(lastSlash + 1);
        }

        function subscriptionChanged() {
            ascApi.getPolicyAssignments(vm.selectedSubscription.rowKey).then(function (data) {
                vm.policyAssignments = data.value;
            });
        }
    }
})();