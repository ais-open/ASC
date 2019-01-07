(function () {
    'use strict';

    angular
        .module('enrollApp')
        .controller('EnrollmentSupportCntrl', EnrollmentSupportCntrl);

    EnrollmentSupportCntrl.$inject = ['$uibModalInstance', 'enrollmentSupportService'];

    function EnrollmentSupportCntrl($uibModalInstance, enrollmentSupportService) {
        /* jshint validthis:true */

        var vm = this;
        vm.firstName = '';
        vm.lastName = '';
        vm.company = '';
        vm.title = '';
        vm.email = '';
        vm.phone = '';
        vm.comments = '';
        
        vm.sendMessage = sendMessage;
        vm.cancel = cancel;
        vm.showForm = true;
        vm.showSuccess = false;
        vm.showError = false;

        activate();

        function activate() {
        }

        function sendMessage() {
            enrollmentSupportService.sendEnrollmentSupportRequest(vm).then(
                function (response) {
                    vm.showForm = false;
                    vm.showError = false;
                    vm.showSuccess = true;
            },
            function (response) {
                vm.showForm = false;
                vm.showError = true;
                vm.showSuccess = false;
            });
            
        }

        function cancel() {
            $uibModalInstance.dismiss();
        }
    }
})();
