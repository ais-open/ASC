(function () {
    'use strict';

    angular.module('ascApp').controller('BlueprintAssignmentsCtrl', BlueprintAssignmentsCtrl);
    BlueprintAssignmentsCtrl.$inject = ['$uibModal', '$state', 'initialData', 'ascApi', 'toastr'];

    /* @ngInject */
    function BlueprintAssignmentsCtrl($uibModal, $state, initialData, ascApi, toastr) {
        /* jshint validthis: true */
        var vm = this;
        vm.lodash = _;
        vm.blueprintAssignments = [];
        vm.selectedSubscription = null;
        vm.subscriptionId = "";
        vm.subscriptions = initialData;
        vm.getBlueprintAssignments = getBlueprintAssignments;
        vm.viewDetails = viewDetails;
        vm.update = update;
        activate();

        function activate() {
            if (vm.subscriptions && vm.subscriptions.length > 0) {
                vm.selectedSubscription = vm.subscriptions[0];
                vm.subscriptionId = vm.selectedSubscription.rowKey;
                getBlueprintAssignments();
            }
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
                    }
                },
                size: 'lg'
            });
        }

        function update() {

        }
    }
})();