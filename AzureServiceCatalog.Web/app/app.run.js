(function () {
    'use strict';

    angular.module('ascApp').run(['$state', '$rootScope', '$window', 'identityInfo', 'authorizationCheckerService', appRun]);


    function appRun($state, $rootScope, $window, identityInfo, authorizationChecker) {
        // Include $state to kick start the router.
        $rootScope.$on('$stateChangeStart', function (event, toState) {
            //console.log('***stateChangeStart', identityInfo, toState);
            authorizationChecker.checkAuthorized(event, toState);

            if (toState.name === 'activation') {
                return;
            }
            if (identityInfo.isActivation) {
                event.preventDefault();
                $state.go('activation');
                return;
            }

            if (identityInfo.isActivationLogin && identityInfo.isAuthenticated) {
                event.preventDefault();
                $state.go('activation');
                return;
            }

            if (!identityInfo.directoryName && !_.includes(['activation', 'activation-login', 'dashboard'], toState.name)) {
                console.log('***no directory name check!!!');
                if (toState.name === 'dashboard') {
                    return;
                }
                event.preventDefault();
                $window.location.href = '/';
                return;
            }
        });
    }
})();