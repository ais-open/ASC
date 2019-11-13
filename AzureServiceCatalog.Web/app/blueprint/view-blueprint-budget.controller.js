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
        vm.blueprintAssignmentOperation = null;
        vm.getBlueprintAssignmentOperations = getBlueprintAssignmentOperations;
        vm.getUsageData = getUsageData;
        vm.options = {
            chart: {
                type: 'lineChart',
                height: 300,
                width: 400,
                margin: {
                    top: 40,
                    right: 20,
                    bottom: 50,
                    left: 65
                },
                x: function (d) {
                    if (d !== null && d !== undefined) {
                        return new Date(d.x);
                    }
                    else {
                        return 0;
                    }
                },
                y: function (d) {
                    if (d !== null && d !== undefined) {
                        return d.y;
                    }
                    else {
                        return 0;
                    }
                },

                color: d3.scale.category10().range(),
                duration: 300,
                useInteractiveGuideline: true,
                clipVoronoi: false,

                xAxis: {
                    axisLabel: 'Usage Date',
                    tickFormat: function (d) {
                        return d3.time.format('%m/%d/%y')(new Date(d));
                    },
                    showMaxMin: false
                },

                yAxis: {
                    axisLabel: 'Cost ($)',
                    tickFormat: function (d) {
                        return '$' + d.toString(); //d3.format(',.1%')(d);
                    },
                    axisLabelDistance: -10
                },

            },
            title: {
                enable: true,
                text: 'Overall Cost Trend over Time'
            }
        };

        vm.doughnutOptions = {
            chart: {
                type: "pieChart",
                height: 450,
                width: 500,
                donut: true,
                showLabels: true,
                x: function (d) { return d.xValue; },
                y: function (d) { return d.yValue; },
                pie: {
                    labelType: function (d, i) { return d.data.text;}
                },
                duration: 500,
                legend: {
                    "margin": {
                        "top": 5,
                        "right": 140,
                        "bottom": 5,
                        "left": 0
                    }
                }
            },
            title: {
                enable: true,
                text: 'Cost Distribution by Service'
            }
        }
        activate();

        function activate() {
            if (initialData) {
                vm.budget = initialData;
                vm.getBlueprintAssignmentOperations();
                vm.getUsageData();
            }
        }

        function getBlueprintAssignmentOperations() {
            ascApi.getBlueprintAssignmentOperations(vm.subscriptionId, vm.blueprintAssignmentName).then(function (data) {
                vm.blueprintAssignmentOperation = data.value[0];
                getResourcesFromAssignment(vm.blueprintAssignmentOperation);
            });
        }

        function getResourcesFromAssignment(assignmentOperation) {
            var resources = [];
            var deployments = assignmentOperation.properties.deployments;
            deployments.forEach(function (deployment) {
                if (typeof deployment.result !== 'undefined') {
                    if (deployment.kind === "azureResource") {
                        if (typeof deployment.result.resources !== 'undefined') {
                            resources = resources.concat(deployment.result.resources);
                        }
                    }

                }
            });
        }

        function getUsageData() {
            ascApi.getUsageData(vm.budget).then(function (data) {
                vm.resourceData = [
                    {
                        key: "Cost",
                        values: data.costData,
                        area: true
                    },
                    {
                        key: "Budget",
                        values: data.budgetData,
                        area: true
                    }
                ];
                vm.doughnutData = data.costDoughnutData;
            });
        }
    }
})();