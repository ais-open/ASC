(function () {
    'use strict';

    angular.module('ascApp').controller('TasksPaneCtrl', TasksPaneCtrl);

    TasksPaneCtrl.$inject = ['$rootScope', '$interval', '$uibModal', 'ascApi'];

    /* @ngInject */
    function TasksPaneCtrl($rootScope, $interval, $uibModal, ascApi) {
        /* jshint validthis: true */
        var vm = this;

        vm.dismiss = dismiss;
        vm.tasks = [];

        vm.spinnerOptions = {
            radius: 6,
            lines: 13,
            length: 6,
            width: 3,
            speed: 1.7,
            corners: 1.0,
            trail: 100,
            color: '#38B44A'//'#428bca'
        };

        activate();

        ////////////////

        function activate() {
        }

        function dismiss(task) {
            _.remove(vm.tasks, task);
        }

        $rootScope.$on('tasks.deployment-created', function (event, args) {
            console.log('**just listened for task', args);

            // 1. add a tasks
            var currentTask = {
                name: args.name,
                resourceGroupName: args.details.resourceGroupName,
                subscriptionId: args.details.subscriptionId,
                status: 'Running'
            };
            vm.tasks.push(currentTask);

            // 2. start interval to poll task results
            var stop = $interval(function () {
                ascApi.getDeployment(args.details.resourceGroupName, args.name, args.details.subscriptionId).then(function (data) {
                    console.log('***get deployment', data);
                    currentTask.status = data.provisioningState;
                    currentTask.correlationId = data.correlationId;
                    if (data.provisioningState === 'Succeeded') {
                        $interval.cancel(stop);
                        stop = undefined;
                    }

                    if (!data || data.provisioningState === 'Failed') {
                        $interval.cancel(stop);
                        stop = undefined;
                    }
                });
            }, 3000);

            // 3. when tasks complete, stop (adjust icon accordingly - and show results)
        });
    }
})();