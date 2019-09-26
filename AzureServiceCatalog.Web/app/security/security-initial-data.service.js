(function () {
    'use strict';

    angular.module('ascApp').factory('securityInitialDataService', securityInitialDataService);

    securityInitialDataService.$inject = ['$http', '$q', 'appSpinner', 'appStorage'];

    /* @ngInject */
    function securityInitialDataService($http, $q, appSpinner, appStorage) {
        var service = {
            getData: getData,
            getAllAppEnrolledSubscriptions: getAllAppEnrolledSubscriptions,
            getOrganization: getOrganization
        };
        return service;

        ////////////////

        function getData() {
            appSpinner.showSpinner('Retrieving Data...');
            return $q.all([
                getAllAppEnrolledSubscriptions(),
                getOrganization()
            ]).then(function (results) {
                appSpinner.hideSpinner();
                return {
                    subscriptions: results[0],
                    adGroups: results[1].organizationADGroups
                };
            });
        }

        function getAllAppEnrolledSubscriptions() {
            return httpGet('/api/subscriptions/all-app-enrolled');
        }

        function getOrganization() {
            return httpGet('/api/organization');
        }

        function httpGet(url) {
            return httpExecute(url, 'GET', null);
        }

        function httpExecute(requestUrl, method, data) {
            return $http({
                url: requestUrl,
                method: method,
                data: data,
                headers: {
                    'asc-selected-tenant': appStorage.getSelectedTenant()
                }
                //headers: requestConfig.headers
            }).then(function (response) {
                //console.log('**response from EXECUTE', response);
                return response.data;
            }, function (error) {
                console.log('**Error making HTTP request', error);
                handleError(error, requestUrl);
                return $q.reject();
            });
        }
    }
})();