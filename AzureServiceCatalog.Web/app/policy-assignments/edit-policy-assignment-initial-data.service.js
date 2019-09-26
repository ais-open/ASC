(function () {
    'use strict';

    angular.module('ascApp').factory('editPolicyAssignmentInitialDataService', editPolicyAssignmentInitialDataService);

    editPolicyAssignmentInitialDataService.$inject = ['$http', '$q', 'appSpinner', 'appStorage'];

    /* @ngInject */
    function editPolicyAssignmentInitialDataService($http, $q, appSpinner, appStorage) {
        var service = {
            getData: getData,
            getEnrolledSubscription: getEnrolledSubscription,
            getPolicies: getPolicies,
            getPolicyAssignments: getPolicyAssignments
        };
        return service;

        ////////////////

        function getData(subscriptionId) {
            appSpinner.showSpinner('Retrieving Data...');
            return $q.all([
                getEnrolledSubscription(subscriptionId),
                getPolicies(subscriptionId),
                getPolicyAssignments(subscriptionId)
            ]).then(function (results) {
                return {
                    subscription: results[0],
                    policies: _.filter(results[1], function (item) {
                        appSpinner.hideSpinner();
                        return item.policy.properties.policyType === 'Custom';
                    }),
                    policyAssignments: results[2].value
                };
            });
        }

        function getEnrolledSubscription(subscriptionId) {
            return httpGet('/api/subscriptions/enrolled?subscriptionId=' + subscriptionId);
        }

        function getPolicies(subscriptionId) {
            return httpGet('/api/policies?subscriptionId=' + subscriptionId);
        }

        function getPolicyAssignments(subscriptionId) {
            return httpGet('/api/policy-assignments?subscriptionId=' + subscriptionId);
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