(function () {
    'use strict';

    angular.module('ascApp').controller('ViewBlueprintDetailsCtrl', ViewBlueprintDetailsCtrl);
    ViewBlueprintDetailsCtrl.$inject = ['$state', '$uibModalInstance', 'initialData', 'subscriptionId', 'subscriptionName', 'ascApi', 'toastr'];

    /* @ngInject */
    function ViewBlueprintDetailsCtrl($state, $uibModalInstance, initialData, subscriptionId, subscriptionName, ascApi, toastr) {
        /* jshint validthis: true */
        var vm = this;
        vm.subscriptionId = subscriptionId;
        vm.subscriptionName = subscriptionName;
        vm.blueprint = initialData;
        vm.close = close;
        vm.onAssignButtonClick = onAssignButtonClick;
        vm.createdDate = getFormattedDate(vm.blueprint.createdDate);
        vm.lastModifiedDate = getFormattedDate(vm.blueprint.lastModifiedDate);

        function close() {
            $uibModalInstance.dismiss();
        }

        function onAssignButtonClick() {
            $uibModalInstance.dismiss();
            $state.go('assign-blueprint', {
                subscriptionId: vm.subscriptionId,
                subscriptionName: vm.subscriptionName,
                blueprintName: vm.blueprint.name
            });
        }

        function getFormattedDate(dateToBeFormatted) {
            var date = new Date(dateToBeFormatted);
            var formattedDate = date.toLocaleString();
            return formattedDate;
        }
    }
})();