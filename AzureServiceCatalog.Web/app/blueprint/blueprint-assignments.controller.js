(function () {
    'use strict';

    angular.module('ascApp').controller('BlueprintAssignmentsCtrl', BlueprintAssignmentsCtrl);
    BlueprintAssignmentsCtrl.$inject = ['$uibModal', '$state', 'initialData', 'ascApi', 'toastr'];

    /* @ngInject */
    function BlueprintAssignmentsCtrl($uibModal, $state, initialData, ascApi, toastr) {
        /* jshint validthis: true */
        var vm = this;
        vm.lodash = _;
        vm.blueprintAssignments = initialData;
        vm.viewDetails = viewDetails;
        vm.update = update;
        activate();

        function activate() {
            console.log(initialData);
            if (initialData) {

            }
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