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
        vm.budgets = [];
        vm.blueprintAssignmentsHavingBudget = [];
        vm.selectedSubscription = null;
        vm.subscriptionId = $state.params.subscriptionId;
        vm.subscriptionName = $state.params.subscriptionName;
        vm.subscriptions = null;
        vm.onSubscriptionChange = onSubscriptionChange;
        vm.getBlueprintAssignments = getBlueprintAssignments;
        vm.viewDetails = viewDetails;
        vm.addBudget = addBudget;
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
                getBudgets();
            });
        }

        function getBudgets() {
            ascApi.getBudgets(vm.subscriptionId).then(function (data) {
                vm.budgets = data;
                vm.budgets.forEach(function (budget) {
                    var bpIds = budget.blueprintAssignmentId.split(",");
                    vm.blueprintAssignmentsHavingBudget = vm.blueprintAssignmentsHavingBudget.concat(bpIds);
                });
                //Checking if any budget is associated with this blueprint assignment
                vm.blueprintAssignments.forEach(function (bpAssignment) {
                    var idx = vm.blueprintAssignmentsHavingBudget.findIndex(i => i === bpAssignment.name);
                    if (idx >= 0) {
                        var matchingIdx = vm.blueprintAssignments.findIndex(i => i.name === bpAssignment.name);
                        bpAssignment.isBudgetAssigned = true;
                        vm.blueprintAssignments.splice(matchingIdx, 1, bpAssignment);
                    }
                });
                console.log(vm.blueprintAssignments);
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

        function addBudget(blueprintAssignment) {
            var budgetCode = "";
            vm.budgets.forEach(function (budget) {
                var temp = budget.blueprintAssignmentId.split(",");
                var idx = temp.findIndex(i => i === blueprintAssignment.name);
                if (idx >= 0) {
                    budgetCode = budget.code;
                }
            });
            console.log(budgetCode);
            if (blueprintAssignment.isBudgetAssigned) {
                $state.go('view-blueprint-budget', {
                    subscriptionId: vm.subscriptionId,
                    subscriptionName: vm.subscriptionName,
                    blueprintAssignmentName: blueprintAssignment.name,
                    budgetCode: budgetCode
                });
            } else {
                $uibModal.open({
                    templateUrl: '/app/blueprint/add-blueprint-budget-modal.html',
                    controller: 'AddBlueprintBudgetCtrl',
                    controllerAs: 'vm',
                    resolve: {
                        initialData: ['ascApi', function (ascApi) {
                            return vm.budgets;
                        }],
                        subscriptionId: function () {
                            return vm.subscriptionId;
                        },
                        subscriptionName: function () {
                            return vm.subscriptionName;
                        },
                        blueprintAssignmentName: function () {
                            return blueprintAssignment.name;
                        }
                    },
                    size: 'lg'
                });
            }
        }
    }
})();