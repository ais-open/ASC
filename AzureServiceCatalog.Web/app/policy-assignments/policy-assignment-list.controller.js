(function () {
    'use strict';

    angular.module('ascApp').controller('PolicyAssignmentListCtrl', PolicyAssignmentListCtrl);
    PolicyAssignmentListCtrl.$inject = ['ascApi', 'identityInfo', 'appStorage'];

    /* @ngInject */
    function PolicyAssignmentListCtrl(ascApi, identityInfo, appStorage) {
        /* jshint validthis: true */
        var vm = this;
        vm.getPolicyDefName = getPolicyDefName;
        vm.policyAssignments = [];
        vm.selectedSubscription = null;
        vm.subscriptions = null;
        vm.subscriptionChanged = subscriptionChanged;
        vm.getPolicyAssignmentForSelectedSubcription = getPolicyAssignmentForSelectedSubcription;
        vm.isActivation = identityInfo.isActivation;
        vm.isAuthenticated = identityInfo.isAuthenticated;
        vm.userDetail = appStorage.getUserDetail();
        vm.userHasManageAccess = getUserAccessDetails();
        vm.getEnrolledSubscription = getEnrolledSubscription;

        activate();

        function activate() {
            vm.subscriptions = getEnrolledSubscription();

            if (vm.subscriptions && vm.subscriptions.length > 0) {
                vm.selectedSubscription = vm.subscriptions[0];
                subscriptionChanged();
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
                });
            } else {
                enrolledSubscription = JSON.parse(enrolledSubscription);
            }
            return enrolledSubscription;
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

        function getUserAccessDetails() {
            if (vm.userDetail.canAdmin) {
                return true;
            } else {
                return false;
            }
        }

        function subscriptionChanged() {
            ascApi.getPolicyAssignments(vm.selectedSubscription.rowKey).then(function (data) {
                vm.policyAssignments = data.value;
            });
        }

        function getPolicyAssignmentForSelectedSubcription(rowkey) {
            appStorage.setselectedSubcription(rowkey);            
            ascApi.getPolicyAssignments(rowKey).then(function (data) {
                vm.policyAssignments = data.value;
            });

            
        }
    }
})();