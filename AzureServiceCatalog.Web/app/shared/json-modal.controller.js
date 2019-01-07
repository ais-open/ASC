(function () {
    'use strict';

    angular.module('ascApp').controller('JsonModalCtrl', JsonModalCtrl);

    JsonModalCtrl.$inject = ['$uibModalInstance', 'data'];

    /* @ngInject */
    function JsonModalCtrl($uibModalInstance, data) {
        /* jshint validthis: true */
        var vm = this;
        vm.title = data.title;
        vm.jsonString = angular.toJson(data.json, true);
        vm.ok = ok;
        vm.lodash = _;

        function ok() {
            $uibModalInstance.close();
        }
    }
})();