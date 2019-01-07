(function () {
    'use strict';

    angular.module('ascApp').controller('ManageEnrolledSubscriptions', ManageEnrolledSubscriptions);
    ManageEnrolledSubscriptions.$inject = ['initialData', 'ascApi', 'toastr'];

    /* @ngInject */
    function ManageEnrolledSubscriptions(initialData, ascApi, toastr) {
        /* jshint validthis: true */
        var vm = this;

        vm.save = save;
        vm.subscriptions = initialData;

        activate();

        function activate() { }

        function save() {
            var hasEnrolledSubscription = _.find(vm.subscriptions, { 'isEnrolled': true });
            if (hasEnrolledSubscription) {
                var subscriptionsResource = {
                    subscriptions: vm.subscriptions
                };
                ascApi.saveSubscriptions(subscriptionsResource).then(function(data) {
                    toastr.success('The Subscriptions were saved successfully.', 'Save Successful');
                });
            } else {
                toastr.warning('There must be at least one enrolled subscription.', 'Missing Subscription');
            }
        }
    }
})();