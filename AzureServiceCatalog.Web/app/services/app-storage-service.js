(function () {
    'use strict';

    angular.module('ascApp').factory('appStorage', appStorage);

    appStorage.$inject = ['$window'];

    function appStorage($window) {
        var service = {
            getSelectedTenant: getSelectedTenant,
            setSelectedTenant: setSelectedTenant,
            getUserDetail: getUserDetail,
            setUserDetail: setUserDetail,
            setselectedSubcription: setselectedSubcription,
            getselectedSubscription: getselectedSubscription,
            setEnrolledSubscription: setEnrolledSubscription,
            getEnrolledSubscription: getEnrolledSubscription
        };

        return service;

        function getSelectedTenant() {
            return $window.localStorage.getItem('selected-tenant');
        }

        function setSelectedTenant(tenantId) {
            $window.localStorage.setItem('selected-tenant', tenantId);
        }

        function getUserDetail() {
            return JSON.parse($window.localStorage.getItem('user-detail'));
        }

        function setUserDetail(userDetail) {
            $window.localStorage.setItem('user-detail', JSON.stringify(userDetail));
        }

        function setselectedSubcription(id) {
            window.localStorage.removeItem('selected-subscription');
            $window.localStorage.setItem('selected-subscription', id);
        }

        function getselectedSubscription() {
            return $window.localStorage.getItem('selected-subscription');
        }

        function setEnrolledSubscription(enrolledSubscription) {
            window.localStorage.removeItem('enrolled-subscription');
            $window.localStorage.setItem('enrolled-subscription', enrolledSubscription);
        }

        function getEnrolledSubscription() {
            return $window.localStorage.getItem('enrolled-subscription');
        }
    }
})();