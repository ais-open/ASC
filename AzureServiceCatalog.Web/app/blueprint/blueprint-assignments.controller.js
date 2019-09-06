(function () {
    'use strict';

    angular.module('ascApp').controller('BlueprintAssignmentsCtrl', BlueprintAssignmentsCtrl);
    BlueprintAssignmentsCtrl.$inject = ['$state', 'initialData', 'ascApi', 'toastr'];

    /* @ngInject */
    function BlueprintAssignmentsCtrl($state, initialData, ascApi, toastr) {
        /* jshint validthis: true */
        var vm = this;
        vm.lodash = _;

        activate();

        function activate() {

        }
    }
})();