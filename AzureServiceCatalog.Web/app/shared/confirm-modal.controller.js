(function () {
    'use strict';

    angular.module('ascApp').controller('ConfirmModalCtrl', ConfirmModalCtrl);

    ConfirmModalCtrl.$inject = ['$uibModalInstance', 'data'];

    /* @ngInject */
    function ConfirmModalCtrl($uibModalInstance, data) {
        /* jshint validthis: true */
        var vm = this;

        vm.cancel = cancel;
        vm.ok = ok;
        vm.properties = data;

        function cancel() {
            $uibModalInstance.dismiss();
        }

        function ok() {
            $uibModalInstance.close();
        }
    }
})();