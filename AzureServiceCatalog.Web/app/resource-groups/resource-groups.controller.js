(function () {
    'use strict';

    angular.module('ascApp').controller('ResourceGroupsCtrl', ResourceGroupsCtrl);

    ResourceGroupsCtrl.$inject = ['initialData', 'ascApi'];

    /* @ngInject */
    function ResourceGroupsCtrl(initialData, ascApi) {
        /* jshint validthis: true */
        var vm = this;

        vm.resourceGroups = [];
        vm.subscriptions = initialData;
        vm.selectedItem = null;
        vm.showData = showData;
        vm.subscriptionChanged = subscriptionChanged;
        vm.selectedSubscription = null;


        vm.options = {
            chart: {
                type: 'lineChart',
                height: 450,
                width: 800,
                margin: {
                    top: 20,
                    right: 20,
                    bottom: 50,
                    left: 65
                },
                x: function (d) {
                    if (d !== null && d !== undefined) {
                        return d.x;
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
                average: function (d) {
                    return 2;
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
                    //staggerLabels: true
                },

                yAxis: {
                    axisLabel: 'Cost ($)',
                    tickFormat: function (d) {
                        return '$' + d.toString(); //d3.format(',.1%')(d);
                    },
                    axisLabelDistance: 0
                }
            }
        };

        activate();

        ////////////////

        function activate() {
            if (vm.subscriptions && vm.subscriptions.length > 0) {
                vm.selectedSubscription = vm.subscriptions[0];
                subscriptionChanged();
            }
        }

        function showData(resourceGroup) {
            //ascApi.getResourceGroupResources(resourceGroup.name, vm.selectedSubscription.rowKey).then(function (data) {
            ascApi.getResourceGroupFullData(resourceGroup.name, vm.selectedSubscription.rowKey).then(function (data) {
                console.log('***full resource group data', data);
                vm.selectedItem = {
                    resourceGroup: resourceGroup,
                    resources: data.resourcesUsages
                };
                vm.resourceData = data.chartData;
            });
        }

        function subscriptionChanged() {
            vm.resourceGroups = [];
            vm.resourceData = [];
            vm.selectedItem = null;
            ascApi.getResourceGroupsBySubscription(vm.selectedSubscription.rowKey).then(
                function (data) {
                    if (data !== undefined && data !== null) {
                        vm.resourceGroups = data.value;
                    }
                },
                function (error) {
                    alert(error);
                }
            );
        }
    }
})();