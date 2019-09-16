(function () {
    'use strict';

    angular.module('ascApp').controller('PolicyListCtrl', PolicyListCtrl);
    PolicyListCtrl.$inject = ['identityInfo', 'initialData', 'ascApi', 'appStorage'];

    /* @ngInject */
    function PolicyListCtrl(identityInfo, initialData, ascApi, appStorage) {
        /* jshint validthis: true */
            var vm = this;

            vm.isActivation = identityInfo.isActivation;
            vm.isAuthenticated = identityInfo.isAuthenticated;
            vm.policies = [];
            vm.selectedSubscription = null;
            vm.getPolicyDefName = getPolicyDefName;
            vm.subscriptions = initialData;
            vm.getPolicies = getPolicies;
            vm.userDetail = appStorage.getUserDetail();
            vm.userHasManageAccess = getUserAccessDetails();
            vm.getPoliciesForSelectedSubcription = getPoliciesForSelectedSubcription;

            activate();

        function activate() {
            if (vm.subscriptions && vm.subscriptions.length > 0) {
                vm.selectedSubscription = vm.subscriptions[0];
                getPolicies();
            }
        }

        function getUserAccessDetails() {
            if (vm.userDetail.canCreate || vm.userDetail.canAdmin) {
                return true;
            } else {
                return false;
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

        function getPolicies() {
            ascApi.getPolicies(vm.selectedSubscription.rowKey).then(function (data) {
                vm.policies = _.filter(data, function (item) {
                    return item.policy.properties.policyType === 'Custom';
                });
            });
        }

        function getPoliciesForSelectedSubcription(rowkey) {
            ascApi.getPolicies(rowkey).then(function (data) {
                vm.policies = _.filter(data, function (item) {
                    return item.policy.properties.policyType === 'Custom';
                });
            });
        }
    }
})();