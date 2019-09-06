(function () {
    'use strict';

    angular.module('ascApp').factory('viewBlueprintDetailsInitialDataService', viewBlueprintDetailsInitialDataService);

    viewBlueprintDetailsInitialDataService.$inject = ['$q', 'ascApi'];

    /* @ngInject */
    function viewBlueprintDetailsInitialDataService($q, ascApi) {
        var service = {
            getData: getData
        };
        return service;

        ////////////////

        function getData(blueprint) {
            return $q.all({
                blueprint: blueprint
            });
        }
    }
})();