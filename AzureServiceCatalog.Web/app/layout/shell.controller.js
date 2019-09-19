(function () {
    'use strict';

    angular.module('ascApp').controller('ShellCtrl', ShellCtrl);

    ShellCtrl.$inject = ['$rootScope', '$state', 'identityInfo', 'ascApi', 'appStorage', 'siteMap', '$uibModal'];

    function ShellCtrl($rootScope, $state, identityInfo, ascApi, appStorage, siteMap, $uibModal) {
        var vm = this;

        vm.isActivation = identityInfo.isActivation;
        vm.isAuthenticated = identityInfo.isAuthenticated;
        vm.isBlueprintFeatureEnabled = identityInfo.isBlueprintFeatureEnabled
        vm.isPoliciesFeatureEnabled = identityInfo.isPoliciesFeatureEnabled;
        vm.isProductCatalogFeatureEnabled = identityInfo.isProductCatalogFeatureEnabled;
        vm.shellShowSpinner = false;
        vm.showSpinner = false;
        vm.isState = isState;
        vm.spinnerMessage = 'Retrieving data...';
        vm.showFeedback = showFeedback;
        vm.showNavLinks = true;

        vm.spinnerOptions = {
            radius: 40,
            lines: 8,
            length: 0,
            width: 30,
            speed: 1.7,
            corners: 1.0,
            trail: 100,
            color: '#428bca'
        };

        vm.tasksVisible = false;
        vm.feedbackVisible = true;
        vm.tenants = [];
        vm.userName = '';
        vm.userDetail = {};

        activate();

        function activate() {
            vm.userName = vm.isAuthenticated ? identityInfo.name : '';

            if (vm.isAuthenticated && !identityInfo.isActivation) {
                // The vm.shellShowSpinner is needed because, if the shell initiates a request, we want to shell to "override" all
                // other requests so they don't hide the spinner before the shell request is complete. In other words,
                // vm.shellShowSpinner takes absolute priority.
                vm.shellShowSpinner = true;
                ascApi.getIdentity().then(function (data) {
                    appStorage.setUserDetail(data);
                    vm.userName = data.userName;
                    vm.userDetail = data;
                    vm.shellShowSpinner = false;
                });
            }
        }

        function showFeedback()
        {
            //Show the modal
            var feedbackDialog = $uibModal.open({
                templateUrl: '/app/support/feedback.html',
                controller: 'FeedbackCntrl',
                controllerAs: 'vm'
                //size: 'lg'
            });
        }

        $rootScope.$on('spinner.toggle', function (event, args) {
            vm.showSpinner = args.show;
            if (args.message) {
                vm.spinnerMessage = args.message;
            }
        });

        $rootScope.$on('activation-active', function (event, args) {
            vm.activation = true;
        });

        $rootScope.$on('tasks.deployment-created', function (event, args) {
            vm.tasksVisible = true;
        });

        $rootScope.$on('$stateChangeSuccess', stateChangeSuccess);

        function stateChangeSuccess(event, toState, toParams, fromState, fromParams) {
            var current = getNavState(siteMap, toState.name);
            if (current) {
                if (current.state === 'manage-policy-list' || current.state === 'edit-policy' || current.state === 'product-deployment' || current.state === 'blueprints-home'
                    || current.state === 'assign-blueprint' || current.state === 'blueprint-assignments' || current.state === 'update-blueprint-assignment'
                    || current.state === 'manage-policy-assignments' || current.state === 'edit-policy-assignment')
                {
                    vm.showNavLinks = false;
                }
                else {
                    vm.currentNav = (current.state === 'dashboard' ? null : current.title);
                    vm.parentStates = _(getParents(current)).reverse().value();
                    vm.showNavLinks = true;
                }                
            } else {
                vm.currentNav = '(unknown state)';
                vm.showNavLinks = true;
            }
        }

        function getNavState(states, currentState, parent) {
            var foundState;
            _.forEach(states, function (item) {
                item.parent = parent;
                if (item.state === currentState) {
                    foundState = item;
                    return false;
                } else {
                    if (item.descendants) {
                        foundState = getNavState(item.descendants, currentState, item);
                        if (foundState) {
                            return false;
                        }
                    }
                }
            });
            return foundState;
        }

        function getParents(item) {
            var parentStates = [];
            if (item) {
                var parent = item.parent;
                while (parent) {
                    parentStates.push(parent);
                    parent = parent.parent;
                }
            }
            return parentStates;
        }

        function isState(state) {
            return $state.is(state);
        }
    }
})();
