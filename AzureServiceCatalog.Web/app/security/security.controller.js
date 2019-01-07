(function () {
    'use strict';

    angular.module('ascApp').controller('SecurityCtrl', SecurityCtrl);

    SecurityCtrl.$inject = ['initialData', 'ascApi', 'toastr'];

    /* @ngInject */
    function SecurityCtrl(initialData, ascApi, toastr) {
        /* jshint validthis: true */
        var vm = this;
        vm.adGroups = initialData.adGroups;
        vm.save = save;
        vm.selectedGroups = [];
        vm.subscriptionChanged = subscriptionChanged;
        vm.subscriptions = initialData.subscriptions;
        vm.createCustomRole = createCustomRole;

        activate();

        ////////////////

        function activate() {}

        function createCustomRole() {
            ascApi.createCustomRole(vm.selectedSubscription.id).then(function (data) {
                console.log('**createCustomRole', data);
            });
        }

        function save() {
            var groups = _.map(vm.selectedGroups, function (item) {
                return { name: item.name, id: item.id };
            });
            vm.selectedSubscription.contributorGroups = JSON.stringify(groups);
            var subscriptionsResource = {
                subscriptions: [vm.selectedSubscription]
            };
            ascApi.saveSubscriptions(subscriptionsResource).then(function (data) {
                toastr.success('Authorized groups successfully saved.', 'Save Successful');
            });
        }

        function subscriptionChanged(item, model) {
            vm.selectedGroups = JSON.parse(item.contributorGroups);
        }
    }
})();