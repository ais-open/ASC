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
        vm.getPolicyAssignmentForSelectedSubcription = getPolicyAssignmentForSelectedSubcription;

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
                var policyDefId = policy.properties["policyDefinitionId"];
                var lastSlash = policyDefId.lastIndexOf('/');
                name = policyDefId.substring(lastSlash + 1);
            }
            else {
                name = policy.properties["displayName"];
            }

            return name;
        }

        function subscriptionChanged() {
            ascApi.getPolicyAssignments(vm.selectedSubscription.rowKey).then(function (data) {
                vm.policyAssignments = data.value;
            });
        }

        function getPolicyAssignmentForSelectedSubcription(rowkey) {
            ascApi.setselectedSubcription(rowkey);            
            ascApi.getPolicyAssignments(rowKey).then(function (data) {
                vm.policyAssignments = data.value;
            });

            
        }
    }
})();