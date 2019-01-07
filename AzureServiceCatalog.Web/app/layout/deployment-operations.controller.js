(function () {
    'use strict';

    angular.module('ascApp').controller('DeploymentOperationsCtrl', DeploymentOperationsCtrl);
    DeploymentOperationsCtrl.$inject = ['$uibModalInstance', 'data'];

    /* @ngInject */
    function DeploymentOperationsCtrl($uibModalInstance, data) {
        /* jshint validthis: true */
        var vm = this;

        //console.log('**inside DeploymentOperationsCtrl', data);
        vm.data = data;
        vm.getStatusMessage = getStatusMessage;
        vm.ok = ok;


        function getStatusMessage(message) {
            if (message) {
                var msg = JSON.parse(message);
                return msg.Message;
            }
        }

        function ok() {
            $uibModalInstance.close();
        }
    }
})();