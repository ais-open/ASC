(function () {
    'use strict';

    angular.module('ascApp').factory('newResourceGroupInitialDataService', newResourceGroupInitialDataService);

    newResourceGroupInitialDataService.$inject = ['$q', 'ascApi'];

    /* @ngInject */
    function newResourceGroupInitialDataService($q, ascApi) {
        var service = {
            getData: getData
        };
        return service;

        ////////////////

        function getData(subscriptionId) {
            return $q.all([
                ascApi.getStorageProvider(subscriptionId),
                ascApi.getOrganization()
            ]).then(function (results) {
                var storageAccounts = _.find(results[0].resourceTypes, { resourceType: 'storageAccounts' });
                return {
                    locations: storageAccounts.locations,
                    adGroups: results[1].organizationADGroups
                };
            });
        }
    }
})();