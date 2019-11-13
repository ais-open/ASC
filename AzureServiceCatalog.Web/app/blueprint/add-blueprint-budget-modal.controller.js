(function () {
    'use strict';

    angular.module('ascApp').controller('AddBlueprintBudgetCtrl', AddBlueprintBudgetCtrl);
    AddBlueprintBudgetCtrl.$inject = ['$state', '$uibModalInstance', 'initialData', 'subscriptionId', 'subscriptionName', 'blueprintAssignmentName', 'ascApi', 'toastr'];

    /* @ngInject */
    function AddBlueprintBudgetCtrl($state, $uibModalInstance, initialData, subscriptionId, subscriptionName, blueprintAssignmentName, ascApi, toastr) {
        /* jshint validthis: true */
        var vm = this;
        vm.subscriptionId = subscriptionId;
        vm.subscriptionName = subscriptionName;
        vm.blueprintAssignmentName = blueprintAssignmentName;
        vm.existingBudgets = initialData;
        vm.selectedBudget = null;
        vm.budgetName = "";
        vm.budgetDescription = "";
        vm.budgetAmount = "";
        vm.budgetRepeatFrequency = "";
        vm.budgetStartDate = new Date();
        vm.budgetEndDate = "";
        vm.isCreateNewBudgetClicked = false;
        vm.close = close;
        vm.onCreateButtonClick = onCreateButtonClick;
        vm.onBudgetRepeatFrequencyChange = onBudgetRepeatFrequencyChange;
        vm.saveExistingBudget = saveExistingBudget;
        vm.saveNewBudget = saveNewBudget;

        function close() {
            $uibModalInstance.dismiss();
        }

        function onCreateButtonClick() {
            vm.isCreateNewBudgetClicked = true;
        }

        function onBudgetRepeatFrequencyChange() {
            var date = new Date();
            if (vm.budgetRepeatFrequency === "Monthly") {
                vm.budgetEndDate = new Date(date.setMonth(date.getMonth() + 1));
            } else if (vm.budgetRepeatFrequency === "Quarterly") {
                vm.budgetEndDate = new Date(date.setMonth(date.getMonth() + 3));
            } else if (vm.budgetRepeatFrequency === "Yearly") {
                vm.budgetEndDate = new Date(date.setMonth(date.getMonth() + 12));
            }
        }

        function saveExistingBudget() {
            $uibModalInstance.dismiss();
            console.log(vm.selectedBudget);
            var budgetToBeUpdated = vm.selectedBudget;
            var newBlueprintAssgId = vm.selectedBudget.blueprintAssignmentId + ',' + vm.blueprintAssignmentName;
            budgetToBeUpdated.blueprintAssignmentId = newBlueprintAssgId;
            ascApi.updateBudget(budgetToBeUpdated).then(function (data) {
                if (data.error) {
                    console.log('Error while updating the Budget!', data);
                    toastr.error('Unexpected error while updating the Budget.', 'Error');
                } else {
                    toastr.success('Budget updated successfully.', 'Success');
                }
            });
        }

        function saveNewBudget() {
            $uibModalInstance.dismiss();
            var budget = {
                blueprintAssignmentId: vm.blueprintAssignmentName,
                subscriptionId: vm.subscriptionId,
                name: vm.budgetName,
                description: vm.budgetDescription,
                amount: parseFloat(vm.budgetAmount),
                repeatType: vm.budgetRepeatFrequency,
                startDate: new Date(vm.budgetStartDate),
                endDate: new Date(vm.budgetEndDate)
            }
            console.log(budget);
            ascApi.createBudget(budget).then(function (data) {
                if (data.error) {
                    console.log('Error while creating the Budget!', data);
                    toastr.error('Unexpected error while creating the Budget.', 'Error');
                } else {
                    toastr.success('Budget created successfully.', 'Success');
                }
            });
        }
    }
})();