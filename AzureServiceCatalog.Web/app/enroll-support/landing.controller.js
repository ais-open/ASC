(function () {
    'use strict';

    angular
        .module('enrollApp')
        .controller('LandingCntrl', LandingCntrl);

    LandingCntrl.$inject = ['$uibModal'];

    function LandingCntrl($uibModal) {
        /* jshint validthis:true */
        var vm = this;
        vm.title = 'landing';
        vm.showEnrollmentRequestForm = showEnrollmentRequestForm;

        activate();

        function activate() { }

        function showEnrollmentRequestForm() {
            //Show the modal
            var enrollDialog = $uibModal.open({
                templateUrl: '/app/enroll-support/enrollment-support.html',
                controller: 'EnrollmentSupportCntrl',
                controllerAs: 'vm'
                //size: 'lg'
            });
        }
    }
})();
