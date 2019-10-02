(function () {
    'use strict';

    angular.module('ascApp').controller('ActivationCtrl', ActivationCtrl);
    ActivationCtrl.$inject = ['$rootScope', 'identityInfo', 'ascApi', 'toastr'];

    /* @ngInject */
    function ActivationCtrl($rootScope, identityInfo, ascApi, toastr) {
        /* jshint validthis: true */
        var vm = this;
        vm.activateStep = activateStep;
        vm.activationComplete = activationComplete;
        vm.activationInfo = {};
        vm.activateWelcomeComplete = activateWelcomeComplete;
        //vm.adGroupChanged = false;
        vm.adminGroups = [];
        vm.createGroups = [];
        vm.currentStep = 0;
        vm.filterGroups = filterGroups;
        vm.getProgressClass = getProgressClass;
        vm.hostSubscriptionChanged = hostSubscriptionChanged;
        vm.hostSubscriptionDetected = false;
        vm.locations = [];
        vm.refreshADGroups = refreshADGroups;
        vm.selectedGroupNames = {};
        vm.selectedSubscriptionId = 0;
        vm.step1hostSubscriptionComplete = step1hostSubscriptionComplete;
        vm.step2manageSubscriptionsComplete = step2manageSubscriptionsComplete;
        vm.step3ADGroupsComplete = step3ADGroupsComplete;
        vm.showEnrollmentSuccess = false;
        vm.showManagement = false;
        vm.showGroupPermissionHelp = false;
        vm.isGlobalAdministrator = false;
        vm.isActivatedByAdmin = false;
        vm.states = {
            start: { index: 1, displayName: 'Sign In' },
            hostSubscription: { index: 2, displayName: 'Host Subscription' },
            manageSubscription: { index: 3, displayName: 'Managed Subscriptions' },
            adGroups: { index: 4, displayName: 'AD Security Groups' },
            finish: { index: 5, displayName: 'Review' }
        };
        vm.subscriptionChanged = subscriptionChanged;
        vm.subscriptionDetail = {};
        vm.adminSubscriptions = [];


        activate();
        activateWelcomeComplete();

        function activate() {
            //console.log('***in activation', identityInfo);
            $rootScope.$broadcast('activation-active');

            // Make an exception to put this call here, rather than route resolver, so spinner behaves correctly
            // since Home screen is typically the first page loaded from server.
            if (identityInfo.isAuthenticated) {
                ascApi.getIdentityFull().then(function (data) {
                    console.log('***getIdentityFull in ActivationCtrl', data);
                    vm.subscriptionDetail = data;
                    vm.trimmedOrgDomain = _.replace(vm.subscriptionDetail.organization.verifiedDomain, '.onmicrosoft.com', '');
                    vm.showManagement = true;
                    vm.currentStep = 2;

                    vm.adminGroups = vm.subscriptionDetail.organizationADGroups;
                    vm.createGroups = vm.subscriptionDetail.organizationADGroups;
                    vm.isGlobalAdministrator = vm.subscriptionDetail.isGlobalAdministrator;
                    vm.isActivatedByAdmin = vm.subscriptionDetail.isActivatedByAdmin;
                    
                    // Check if any subscriptions are already hosting
                    var selected = _.find(vm.subscriptionDetail.subscriptions, { 'subscriptionIsConnected': true });

                    // Check if any subscriptions are enrolled
                    var enrolled = _.find(vm.subscriptionDetail.subscriptions, { 'isEnrolled': true });
                    if (selected) {
                        vm.hostSubscriptionDetected = true;
                        vm.showManagement = false;
                        vm.selectedHost = selected;
                    }
                    else if (enrolled) {
                        vm.hostSubscriptionDetected = true;
                        vm.showManagement = false;
                        vm.selectedHost = enrolled;
                    }
                    //else {
                    //    vm.selectedHost = vm.subscriptionDetail.subscriptions[0];
                    //}

                    vm.adminSubscriptions = _.filter(vm.subscriptionDetail.subscriptions, { 'isAdminOfSubscription': true });
                    if (!selected && vm.adminSubscriptions && vm.adminSubscriptions.length > 0) {
                        vm.selectedHost = vm.adminSubscriptions[0];
                    }
                });
            }
        }

        function activateStep(step) {
            vm.currentStep = step.index;
        }

        function activationComplete() {
            if (vm.selectedHost) {
                var org = vm.subscriptionDetail.organization;
                vm.activationInfo.organization = org;

                vm.activationInfo.hostSubscription = vm.selectedHost;
                vm.activationInfo.enrolledSubscriptions = [vm.selectedHost];
                console.log(vm.activationInfo);
            }
            ascApi.saveActivation(vm.activationInfo).then(function (data) {
                vm.showEnrollmentSuccess = true;
            });
        }

        function activateWelcomeComplete() {

            activateStep(vm.states.hostSubscription);
            console.log(vm.currentStep, vm.showManagement, vm.hostSubscriptionDetected);
        }

        function getProgressClass(step) {
            var statusClass;
            if (vm.currentStep === step.index) {
                statusClass = 'active';
            } else if (vm.currentStep > step.index) {
                statusClass = 'complete';
            } else if (vm.currentStep < step.index) {
                statusClass = 'disabled';
            }
            var colWidth = (step.index === 1 || step.index === 5 ? '1' : '3');
            return statusClass + ' col-xs-' + colWidth;
        }

        function hostSubscriptionChanged() {
            ascApi.getStorageProvider(vm.selectedHost.subscriptionId).then(function (data) {
                console.log('**result of getLocations', data);
                var storageAccounts = _.find(data.resourceTypes, { resourceType: 'storageAccounts' });
                vm.locations = storageAccounts.locations;
            });
        }

        function filterGroups(filter, groupType) {
            if (filter) {
                ascApi.getOrganizationGroups(filter).then(function (data) {
                    vm[groupType] = data;
                });
            }
        }

        function refreshADGroups() {
            ascApi.getOrganizationGroups().then(function (data) {
                vm.subscriptionDetail.organizationADGroups = data;
            });
        }

        function step1hostSubscriptionComplete() {

            if (vm.selectedHost) {
                var org = vm.subscriptionDetail.organization;
                vm.activationInfo.organization = org;
                vm.subscriptionDetail.organization.createProductGroup = vm.adminGroups[0];
                vm.subscriptionDetail.organization.adminGroup = vm.adminGroups[0];
                vm.activationInfo.hostSubscription = vm.selectedHost;
                vm.activationInfo.enrolledSubscriptions = [vm.selectedHost];
                activateStep(vm.states.manageSubscription);
                console.log(vm.activationInfo);
                activationComplete();
            } else {
                toastr.warning('You must first select a Host Subscription and specify Resource Group / Location!', 'Incomplete');
            }
        }

        function step2manageSubscriptionsComplete() {
            var enrolled = _.filter(vm.subscriptionDetail.subscriptions, { 'isEnrolled': true });
            if (enrolled.length) {
                vm.activationInfo.enrolledSubscriptions = enrolled;
                activateStep(vm.states.adGroups);
            } else {
                toastr.warning('You must select at least 1 Subscription to be Managed!', 'Incomplete');
            }
        }

        function step3ADGroupsComplete() {
            var org = vm.subscriptionDetail.organization;
            if (org.createProductGroup && org.adminGroup) {
                vm.activationInfo.organization = org;
                activateStep(vm.states.finish);
            } else {
                toastr.warning('You must specify groups for creating products, ' +
                    'deploying resources, and catalog administration', 'Incomplete');
            }
        }

        function subscriptionChanged(item, model) {
            vm.selectedSubscriptionId = item.subscriptionId;
        }
    }
})();