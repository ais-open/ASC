(function () {
    'use strict';

    angular.module('ascApp').controller('ErrDialogBoxCtrl', ErrDialogBoxCtrl);

    ErrDialogBoxCtrl.$inject = ['$uibModalInstance', 'data'];

    /* @ngInject */
    function ErrDialogBoxCtrl($uibModalInstance, data) {
        /* jshint validthis: true */
        var vm = this;
        vm.getDate = getDate;
        vm.close = close;
        vm.properties = data;

        function getDate(requestDate) {
            var date = new Date(requestDate);
            var updatedDate = date.toLocaleString();
            console.log(updatedDate);
            return updatedDate;
        }

        function close() {
            $uibModalInstance.close();
        }
    }
})();
