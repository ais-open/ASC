(function () {
    'use strict';

    angular
        .module('enrollApp')
        .factory('enrollmentSupportService', enrollmentSupportService);

    enrollmentSupportService.$inject = ['$http', '$q', '$window'];

    function enrollmentSupportService($http, $q, $window) {
        var service = {
            sendEnrollmentSupportRequest: sendEnrollmentSupportRequest
        };

        return service;

        function sendEnrollmentSupportRequest(vm) {
            return httpPost('/api/enrollmentsupport', vm);
        }

        function httpPost(url, data) {
            return httpExecute(url, 'POST', data);
        }

        function httpExecute(requestUrl, method, data, suppressSpinner) {
            

            return $http({
                url: requestUrl,
                method: method,
                data: data
            }).then(function (response) {
                //console.log('**response from EXECUTE', response);
                return response.data;
            }, function (error) {
                console.log('**Error making HTTP request', error);
                return $q.reject();
            });
        }

    }
})();