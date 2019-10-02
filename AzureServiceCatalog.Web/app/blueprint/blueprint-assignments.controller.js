(function () {
    'use strict';

    angular.module('ascApp').controller('BlueprintAssignmentsCtrl', BlueprintAssignmentsCtrl);
    BlueprintAssignmentsCtrl.$inject = ['$uibModal', '$state', 'ascApi', 'appStorage'];

    /* @ngInject */
    function BlueprintAssignmentsCtrl($uibModal, $state, ascApi, appStorage) {
        /* jshint validthis: true */
        var vm = this;
        vm.lodash = _;
        vm.blueprintAssignments = [];
        vm.selectedSubscription = null;
        vm.subscriptionId = $state.params.subscriptionId;
        vm.subscriptionName = $state.params.subscriptionName;
        vm.subscriptions = null;
        vm.onSubscriptionChange = onSubscriptionChange;
        vm.getBlueprintAssignments = getBlueprintAssignments;
        vm.viewDetails = viewDetails;
        //vm.update = update;
        vm.getEnrolledSubscription = getEnrolledSubscription;
        activate();

        function activate() {
            vm.subscriptions = getEnrolledSubscription();

            if (vm.subscriptions && vm.subscriptions.length > 0) {
                vm.selectedSubscription = vm.subscriptions[0];
                vm.subscriptionId = vm.selectedSubscription.rowKey;
                getBlueprintAssignments();
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

        function onSubscriptionChange(rowKey) {
            appStorage.setselectedSubcription(rowkey);  
            vm.subscriptionId = rowKey;
            getBlueprintAssignments();
        }

        function getBlueprintAssignments() {
            ascApi.getBlueprintAssignments(vm.subscriptionId).then(function (data) {
                vm.blueprintAssignments = data;
            });
        }

        function viewDetails(blueprintAssignment) {
            $uibModal.open({
                templateUrl: '/app/blueprint/view-blueprint-assignment-details-modal.html',
                controller: 'ViewBlueprintAssignmentDetailsCtrl',
                controllerAs: 'vm',
                resolve: {
                    initialData: ['ascApi', function (ascApi) {
                        return blueprintAssignment;
                    }],
                    subscriptionId: function () {
                        return vm.subscriptionId;
                    },
                    subscriptionName: function () {
                        return vm.subscriptionName;
                    }
                },
                size: 'lg'
            });
        }

        //function update() {

        //}
    }
})();