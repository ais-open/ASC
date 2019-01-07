(function () {
    'use strict';

    angular.module('ascApp').factory('securityInitialDataService', securityInitialDataService);

    securityInitialDataService.$inject = ['$q', 'ascApi'];

    /* @ngInject */
    function securityInitialDataService($q, ascApi) {
        var service = {
            getData: getData
        };
        return service;

        ////////////////

        function getData() {
            return $q.all([
                ascApi.getAllAppEnrolledSubscriptions(),
                ascApi.getOrganization()
            ]).then(function (results) {
                return {
                    subscriptions: results[0],
                    adGroups: results[1].organizationADGroups
                };
            });
        }
    }
})();