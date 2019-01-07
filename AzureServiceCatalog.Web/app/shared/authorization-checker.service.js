(function () {
    'use strict';

    angular.module('ascApp').factory('authorizationCheckerService', authorizationCheckerService);

    authorizationCheckerService.$inject = ['$state', 'appStorage'];

    function authorizationCheckerService($state, appStorage) {

        var service = {
            checkAuthorized: checkAuthorized
        };
        return service;

        function checkAuthorized(event, state) {
            var isAuthorized = true;
            var userDetail = appStorage.getUserDetail();
            if (state.createPermissionRequired) {
                isAuthorized = userDetail.canCreate;
            } else if (state.deployPermissionRequired) {
                isAuthorized = userDetail.canDeploy;
            } else if (state.adminPermissionRequired) {
                isAuthorized = userDetail.canAdmin;
            }

            if (!isAuthorized) {
                event.preventDefault();
                $state.go('home');
            }
            return isAuthorized;
        }
    }
})();