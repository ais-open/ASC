(function () {
    'use strict';

    angular.module('ascApp').controller('BlueprintsHomeCtrl', BlueprintsHomeCtrl);
    BlueprintsHomeCtrl.$inject = ['initialData', 'ascApi'];

    /* @ngInject */
    function BlueprintsHomeCtrl(initialData, ascApi) {
        /* jshint validthis: true */
        var vm = this;
        vm.blueprints = [];
        vm.selectedSubscription = null;
        vm.subscriptionId = "";
        vm.subscriptions = initialData;
        vm.subscriptionChanged = subscriptionChanged;
        vm.getLastModifiedDate = getLastModifiedDate;
        activate();

        function activate() {
            if (vm.subscriptions && vm.subscriptions.length > 0) {
                vm.selectedSubscription = vm.subscriptions[0];
                vm.subscriptionId = vm.selectedSubscription.rowKey;
                subscriptionChanged();
            }
        }

        function subscriptionChanged() {
            ascApi.getBlueprints(vm.selectedSubscription.rowKey).then(function (data) {
                vm.blueprints = data;
            });
        }

        function getLastModifiedDate(lastModifiedDate) {
            var newDate = new Date(lastModifiedDate);
            var updatedLastModifiedDate = newDate.toLocaleDateString();
            return updatedLastModifiedDate;
        }
    }
})();