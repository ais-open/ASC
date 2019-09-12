(function () {
    'use strict';

    angular.module('ascApp').controller('ViewBlueprintAssignmentDetailsCtrl', ViewBlueprintAssignmentDetailsCtrl);
    ViewBlueprintAssignmentDetailsCtrl.$inject = ['$state', '$uibModalInstance', 'initialData', 'subscriptionId', 'ascApi', 'toastr'];

    /* @ngInject */
    function ViewBlueprintAssignmentDetailsCtrl($state, $uibModalInstance, initialData, subscriptionId, ascApi, toastr) {
        /* jshint validthis: true */
        var vm = this;
        vm.subscriptionId = subscriptionId;
        vm.blueprintAssignment = initialData;
        vm.close = close;
        vm.onAssignButtonClick = onAssignButtonClick;
        vm.onViewAssignmentsButtonClick = onViewAssignmentsButtonClick;
        vm.createdDate = getFormattedDate(vm.blueprintAssignment.createdDate);
        vm.lastModifiedDate = getFormattedDate(vm.blueprintAssignment.lastModifiedDate);

        function close() {
            $uibModalInstance.dismiss();
        }

        function onAssignButtonClick() {

        }

        function onViewAssignmentsButtonClick() {

        }

        function getFormattedDate(dateToBeFormatted) {
            var date = new Date(dateToBeFormatted);
            var formattedDate = date.toLocaleString();
            return formattedDate;
        }
    }
})();