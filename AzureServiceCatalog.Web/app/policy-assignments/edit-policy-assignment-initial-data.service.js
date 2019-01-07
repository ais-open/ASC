(function () {
    'use strict';

    angular.module('ascApp').factory('editPolicyAssignmentInitialDataService', editPolicyAssignmentInitialDataService);

    editPolicyAssignmentInitialDataService.$inject = ['$q', 'ascApi'];

    /* @ngInject */
    function editPolicyAssignmentInitialDataService($q, ascApi) {
        var service = {
            getData: getData
        };
        return service;

        ////////////////

        function getData(subscriptionId) {
            return $q.all([
                ascApi.getEnrolledSubscription(subscriptionId),
                ascApi.getPolicies(subscriptionId),
                ascApi.getPolicyAssignments(subscriptionId)
            ]).then(function (results) {
                return {
                    subscription: results[0],
                    policies: _.filter(results[1], function (item) {
                        return item.policy.properties.policyType === 'Custom';
                    }),
                    policyAssignments: results[2].value
                };
            });
        }
    }
})();