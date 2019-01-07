(function () {
    'use strict';

    angular.module('ascApp').controller('SelectGroupCtrl', SelectGroupCtrl);
    SelectGroupCtrl.$inject = ['$uibModalInstance', 'data'];

    /* @ngInject */
    function SelectGroupCtrl($uibModalInstance, data) {
        /* jshint validthis: true */
        var vm = this;

        vm.cancel = cancel;
        vm.data = data;
        vm.ok = ok;

        function cancel() {
            $uibModalInstance.dismiss();
        }

        function ok() {
            $uibModalInstance.close(vm.selected);
        }
    }
})();