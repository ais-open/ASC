(function () {
    'use strict';

    angular.module('ascApp').factory('taskMgr', taskMgr);

    taskMgr.$inject = ['$rootScope'];

    function taskMgr($rootScope) {
        var service = {
            addDeployTask: addDeployTask
        };

        return service;


        function addDeployTask(resourceGroupName, deploymentName, subscriptionId) {
            $rootScope.$broadcast('tasks.deployment-created',
            {
                name: deploymentName,
                details: { resourceGroupName: resourceGroupName, subscriptionId: subscriptionId }
            });
        }
    }
})();