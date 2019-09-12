(function () {
    'use strict';

    angular.module('ascApp').controller('BlueprintAssignmentsCtrl', BlueprintAssignmentsCtrl);
    BlueprintAssignmentsCtrl.$inject = ['$state', 'initialData', 'ascApi', 'toastr'];

    /* @ngInject */
    function BlueprintAssignmentsCtrl($state, initialData, ascApi, toastr) {
        /* jshint validthis: true */
        var vm = this;
        vm.lodash = _;
        vm.blueprintAssignments = initialData;
        vm.viewDetails = viewDetails;
        vm.update = update;
        activate();

        function activate() {
            console.log(initialData);
            if (initialData) {

            }
        }

        function viewDetails() {

        }

        function update() {

        }
    }
})();