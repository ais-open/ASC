(function () {
    'use strict';

    angular.module('ascApp').controller('LoginCtrl', LoginCtrl);
    LoginCtrl.$inject = ['identityInfo'];

    /* @ngInject */
    function LoginCtrl(identityInfo) {
        /* jshint validthis: true */
        var vm = this;
        vm.directoryMissing = true;
        vm.identityInfo = identityInfo;
        console.log('**identityInfo', identityInfo);
        activate();

        function activate() {
            vm.msLoginUrl = '/account/signin?directoryName=' + identityInfo.directoryName + '&isMSA=true';
            if (identityInfo.directoryName) {
                vm.directoryMissing = false;
            }
        }
    }
})();