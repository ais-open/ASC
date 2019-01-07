(function () {
    'use strict';

    angular.module('ascApp').controller('ActivationLoginCtrl', ActivationLoginCtrl);

    ActivationLoginCtrl.$inject = ['ascApi'];

    /* @ngInject */
    function ActivationLoginCtrl(ascApi) {
        /* jshint validthis: true */
        var vm = this;
        vm.checkDirectory = checkDirectory;
        vm.domain = null;
        vm.showAlreadyEnrolled = false;
        vm.showLoginOptions = false;

        activate();

        ////////////////

        function activate() { }

        function checkDirectory() {
            ascApi.getOrganizationByDomain(vm.domain).then(function (data) {
                if (data) {
                    vm.trimmedOrgDomain = _.replace(vm.domain, '.onmicrosoft.com', '');
                    vm.showAlreadyEnrolled = true;
                } else {
                    vm.showLoginOptions = true;
                }
            });
        }
    }
})();