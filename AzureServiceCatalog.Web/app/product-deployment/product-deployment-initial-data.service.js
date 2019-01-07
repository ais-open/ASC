(function () {
    'use strict';

    angular.module('ascApp').factory('productDeploymentInitialDataService', productDeploymentInitialDataService);

    productDeploymentInitialDataService.$inject = ['$q', 'ascApi'];

    /* @ngInject */
    function productDeploymentInitialDataService($q, ascApi) {
        var service = {
            getData: getData
        };
        return service;

        ////////////////

        function getData(productId) {
            return $q.all([
                ascApi.getTemplate(productId),
                ascApi.getEnrolledSubscriptions()
            ]).then(function (results) {
                return {
                    product: results[0],
                    subscriptions: results[1]
                };
            });
        }
    }
})();