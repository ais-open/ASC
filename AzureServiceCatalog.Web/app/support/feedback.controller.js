(function () {
    'use strict';

    angular
        .module('ascApp')
        .controller('FeedbackCntrl', FeedbackCntrl);

    FeedbackCntrl.$inject = ['$uibModalInstance', 'ascApi'];

    function FeedbackCntrl($uibModalInstance, ascApi) {
        /* jshint validthis:true */

        var vm = this;
        vm.subject = '';
        vm.comments = '';
        vm.email = '';
        vm.name = '';
        vm.sendMessage = sendMessage;
        vm.cancel = cancel;
        vm.showForm = true;
        vm.showSuccess = false;
        vm.showError = false;

        activate();

        function activate() {
            ascApi.getDefaultFeedbackInfo().then(
                function (response) {
                    if (response !== null)
                    {
                        vm.name = response.name;
                        vm.email = response.email;
                    }
                },
                function (response) { });
        }

        function sendMessage() {
            ascApi.sendFeedback(vm).then(
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
