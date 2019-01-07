(function () {
    'use strict';

    angular.module('ascApp').controller('NewResourceGroupCtrl', NewResourceGroupCtrl);
    NewResourceGroupCtrl.$inject = ['$uibModalInstance', 'initialData', 'subscriptionId', 'ascApi', 'toastr'];

    /* @ngInject */
    function NewResourceGroupCtrl($uibModalInstance, initialData, subscriptionId, ascApi, toastr) {
        /* jshint validthis: true */
        var vm = this;

        vm.adGroups = initialData.adGroups;
        vm.locations = initialData.locations;
        vm.cancel = cancel;
        vm.ok = ok;

        function cancel() {
            $uibModalInstance.dismiss();
        }

        function ok() {
            var regex = /^[A-Za-z0-9._\(\)\-]+[A-Za-z0-9\(\)]$/;
            if (!regex.test(vm.newResourceGroupName)) {
                var suggestedName = _.kebabCase(vm.newResourceGroupName.name);
                var msg = 'You entered an invalid resource group name (try this instead: "' + suggestedName + '")';
                toastr.warning(msg, 'Invalid Name');
                return;
            }

            if (vm.newResourceGroupName && vm.location && vm.selectedGroups) {
                var newResourceGroup = {
                    resourceGroupName: vm.newResourceGroupName,
                    location: vm.location,
                    subscriptionId: subscriptionId,
                    contributorGroups: vm.selectedGroups
                };

                ascApi.createResourceGroup(newResourceGroup).then(function (data) {
                    $uibModalInstance.close(newResourceGroup);
                });
            } else {
                toastr.warning('You must specify a Resource Group Name, Location, and associated AD Groups!', 'Incomplete');
            }
        }
    }
})();