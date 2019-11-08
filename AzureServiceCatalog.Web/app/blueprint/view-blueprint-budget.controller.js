(function () {
    'use strict';

    angular.module('ascApp').controller('ViewBlueprintBudgetCtrl', ViewBlueprintBudgetCtrl);
    ViewBlueprintBudgetCtrl.$inject = ['$state', 'initialData', 'ascApi', 'toastr', 'dialogsService'];

    /* @ngInject */
    function ViewBlueprintBudgetCtrl($state, initialData, ascApi, toastr, dialogs) {
        /* jshint validthis: true */
        var vm = this;
        vm.lodash = _;
        vm.subscriptionId = $state.params.subscriptionId;
        vm.subscriptionName = $state.params.subscriptionName;
        vm.blueprintAssignmentName = $state.params.blueprintAssignmentName;
        vm.budgetCode = $state.params.budgetCode;
        vm.budget = null;

        activate();

        function activate() {
            console.log(initialData);
            if (initialData) {
                vm.budget = initialData;

            }
        }
    }
})();