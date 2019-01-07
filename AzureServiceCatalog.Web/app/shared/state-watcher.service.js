(function () {
    'use strict';

    angular.module('ascApp').factory('stateWatcherService', stateWatcherService);

    stateWatcherService.$inject = ['$rootScope'];

    function stateWatcherService($rootScope) {

        $rootScope.$on('$stateChangeStart', stateChangeStart);
        $rootScope.$on('$stateChangeSuccess', stateChangeSuccess);
        $rootScope.$on('$stateChangeError', stateChangeError);
        $rootScope.$on('$stateNotFound', stateNotFound);


        function stateChangeStart(event, toState, toParams, fromState, fromParams) {
            console.log('state change start', event, toState, toParams, fromState, fromParams);
        }

        function stateChangeSuccess(event, toState, toParams, fromState, fromParams) {
            console.log('state change success', event, toState, toParams, fromState, fromParams);
        }

        function stateChangeError(event, toState, toParams, fromState, fromParams, error) {
            console.log('state change error', event, toState, toParams, fromState, fromParams, error);
        }

        function stateNotFound(event, unfoundState, fromState, fromParams) {
            console.log('state not found', event, unfoundState, fromState, fromParams);
        }

        var service = {};
        return service;
    }
})();