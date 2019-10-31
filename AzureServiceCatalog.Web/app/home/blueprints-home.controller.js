(function () {
    'use strict';

    angular.module('ascApp').controller('BlueprintsHomeCtrl', BlueprintsHomeCtrl);
    BlueprintsHomeCtrl.$inject = ['$uibModal', 'ascApi', 'appStorage'];

    /* @ngInject */
    function BlueprintsHomeCtrl($uibModal, ascApi, appStorage) {
        /* jshint validthis: true */
        var vm = this;
        vm.lodash = _;
        vm.blueprints = [];
        vm.selectedSubscription = null;
        vm.subscriptionId = "";
        vm.portalUrlForBlueprints = "";
        vm.extensionForUrl = "blade/Microsoft_Azure_Policy/BlueprintsMenuBlade/Blueprints";
        vm.subscriptions = null;
        vm.onSubscriptionChange = onSubscriptionChange;
        vm.getPortalUrlForBlueprints = vm.getPortalUrlForBlueprints;
        vm.getBlueprints = getBlueprints;
        vm.viewDetails = viewDetails;
        //vm.getLastModifiedDate = getLastModifiedDate;
        vm.getSubscriptionById = getSubscriptionById;
        vm.getEnrolledSubscription = getEnrolledSubscription;
        activate();

        function activate() {
            vm.subscriptions = getEnrolledSubscription();

            if (vm.subscriptions && vm.subscriptions.length > 0) {
                var subId = appStorage.getselectedSubscription();
                if (subId !== "" || subId !== null) {
                    vm.selectedSubscription = vm.getSubscriptionById(vm.subscriptions, 'rowKey', subId);
                    vm.subscriptionId = subId
                }
                if (vm.selectedSubscription === null) {
                    vm.selectedSubscription = vm.subscriptions[0];
                    appStorage.setselectedSubcription(vm.selectedSubscription.rowKey); 
                    vm.subscriptionId = vm.selectedSubscription.rowKey;
                }
                getBlueprints();
            }
        }

        function getEnrolledSubscription() {
            var enrolledSubscription = appStorage.getEnrolledSubscription();

            if (enrolledSubscription === null) {
                enrolledSubscription = [];
                ascApi.getEnrolledSubscriptions().then(function (data) {
                    for (var i = 0, length = data.length; i < length; i++) {
                        enrolledSubscription.push(data[i]);
                    }
                    appStorage.setEnrolledSubscription(JSON.stringify(enrolledSubscription));
                    if (enrolledSubscription.length >= 0) {
                        vm.selectedSubscription = vm.subscriptions[0];
                        onSubscriptionChange(enrolledSubscription[0].rowKey);
                    }
                });
            } else {
                enrolledSubscription = JSON.parse(enrolledSubscription);
            }
            return enrolledSubscription;
        }

        function getSubscriptionById(array, key, value) {
            for (var i = 0; i < array.length; i++) {
                if (array[i][key] === value) {
                    return array[i];
                }
            }
            return null;
        }

        function onSubscriptionChange(rowkey) {
            appStorage.setselectedSubcription(rowkey); 
            vm.subscriptionId = rowkey;
            getBlueprints();
        }

        function getPortalUrlForBlueprints() {
            ascApi.getPortalUrl(vm.extensionForUrl, 'Common').then(function (data) {
                vm.portalUrlForBlueprints = data;
            });
        }

        function getBlueprints() {
            ascApi.getBlueprints(vm.subscriptionId).then(function (data) {
                vm.blueprints = data;
                getPortalUrlForBlueprints();
            });
        }

        function viewDetails(blueprint) {
            $uibModal.open({
                templateUrl: '/app/blueprint/view-blueprint-details-modal.html',
                controller: 'ViewBlueprintDetailsCtrl',
                controllerAs: 'vm',
                resolve: {
                    initialData: ['ascApi', function (ascApi) {
                        return blueprint;
                    }],
                    subscriptionId: function () {
                        return vm.subscriptionId;
                    },
                    subscriptionName: function () {
                        return vm.selectedSubscription.displayName;
                    }
                },
                size: 'lg'
            });

        }

        //function getLastModifiedDate(lastModifiedDate) {
        //    var newDate = new Date(lastModifiedDate);
        //    var updatedLastModifiedDate = newDate.toLocaleDateString();
        //    return updatedLastModifiedDate;
        //}
    }
})();