(function () {
    'use strict';

    angular.module('ascApp').controller('AssignBlueprintCtrl', AssignBlueprintCtrl);
    AssignBlueprintCtrl.$inject = ['initialData', 'ascApi'];

    /* @ngInject */
    function AssignBlueprintCtrl(initialData, ascApi) {
        /* jshint validthis: true */
        var vm = this;


        activate();

        function activate() {
            console.log('in activate');

        }
    }
})();