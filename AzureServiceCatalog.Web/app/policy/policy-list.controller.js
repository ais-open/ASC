(function () {
    'use strict';

    angular.module('ascApp').controller('PolicyListCtrl', PolicyListCtrl);
    PolicyListCtrl.$inject = ['identityInfo', 'ascApi', 'appStorage'];

    /* @ngInject */
    function PolicyListCtrl(identityInfo, ascApi, appStorage) {
        /* jshint validthis: true */
            var vm = this;

            vm.isActivation = identityInfo.isActivation;
            vm.isAuthenticated = identityInfo.isAuthenticated;
            vm.policies = [];
            vm.selectedSubscription = null;
            vm.getPolicyDefName = getPolicyDefName;
            vm.subscriptions = null;
            vm.getPolicies = getPolicies;
            vm.userDetail = appStorage.getUserDetail();
            vm.userHasManageAccess = getUserAccessDetails();
            vm.getPoliciesForSelectedSubcription = getPoliciesForSelectedSubcription;
            vm.getSubscriptionById = getSubscriptionById;
            vm.getEnrolledSubscription = getEnrolledSubscription;

            activate();

        function activate() {
            vm.subscriptions = getEnrolledSubscription();

            if (vm.subscriptions && vm.subscriptions.length > 0) {
            
                var subId = appStorage.getselectedSubscription();

                if (subId !== "" || subId !== null)
                    vm.selectedSubscription = vm.getSubscriptionById(vm.subscriptions, 'rowKey', subId);

                if (vm.selectedSubscription === null) {
                    vm.selectedSubscription = vm.subscriptions[0];
                }
                if (vm.userHasManageAccess) {
                    getPolicies();
                }
            }
        }

        function getEnrolledSubscription() {
            var enrolledSubscription = appStorage.getEnrolledSubscription();

            if (enrolledSubscription === null) {
                enrolledSubscription = [];
                ascApi.getEnrolledSubscriptions().then(function (data) {
                    for (var i = 0, length = data.length; i < length; i++) {
                        enrolledSubscription.push(data[i]);
                    }
                    appStorage.setEnrolledSubscription(JSON.stringify(enrolledSubscription));
                    if (enrolledSubscription.length >= 0) {
                        vm.selectedSubscription = vm.subscriptions[0];
                        vm.subscriptionId = enrolledSubscription[0].rowKey;
                        getPoliciesForSelectedSubcription(enrolledSubscription[0].rowKey);
                    }
                });
            } else {
                enrolledSubscription = JSON.parse(enrolledSubscription);
            }
            return enrolledSubscription;
        }


        function getSubscriptionById(array, key, value) {
            for (var i = 0; i < array.length; i++) {
                if (array[i][key] === value) {
                    return array[i];
                }
            }
            return null;
        }

        function getUserAccessDetails() {
            if (vm.userDetail.canAdmin) {
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
            appStorage.setselectedSubcription(rowkey);    
            ascApi.getPolicies(rowkey).then(function (data) {
                vm.policies = _.filter(data, function (item) {
                    return item.policy.properties.policyType === 'Custom';
                });
            });
        }
    }
})();