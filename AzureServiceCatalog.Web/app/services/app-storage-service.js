(function () {
    'use strict';

    angular.module('ascApp').factory('appStorage', appStorage);

    appStorage.$inject = ['$window'];

    function appStorage($window) {
        var service = {
            getSelectedTenant: getSelectedTenant,
            setSelectedTenant: setSelectedTenant,
            getUserDetail: getUserDetail,
            setUserDetail: setUserDetail
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
    }
})();