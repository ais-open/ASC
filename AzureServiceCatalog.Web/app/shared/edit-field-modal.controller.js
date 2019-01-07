(function () {
    'use strict';

    angular.module('ascApp').controller('EditFieldModalCtrl', EditFieldModalCtrl);

    EditFieldModalCtrl.$inject = ['$uibModalInstance', 'data'];

    /* @ngInject */
    function EditFieldModalCtrl($uibModalInstance, data) {
        /* jshint validthis: true */
        var vm = this;

        vm.cancel = cancel;
        vm.ok = ok;
        vm.lodash = _;
        vm.fileSelectionChanged = fileSelectionChanged;
        vm.field = data;
        vm.origData = angular.copy(data.field);

        function fileSelectionChanged(fileBinaryString) {
            vm.field.fieldValue = fileBinaryString;
        }

        function cancel() {
            $uibModalInstance.dismiss();
        }

        function ok() {
            $uibModalInstance.close(vm.field);
        }
    }
})();