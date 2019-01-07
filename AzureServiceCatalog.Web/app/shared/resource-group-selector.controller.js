(function () {
    'use strict';

    angular.module('ascApp').controller('ResourceGroupSelectorCtrl', ResourceGroupSelectorCtrl);

    ResourceGroupSelectorCtrl.$inject = ['ascApi', 'toastr'];

    /* @ngInject */
    function ResourceGroupSelectorCtrl(ascApi, toastr) {
        /* jshint validthis: true */
        var vm = this;
        vm.newRGVisible = false;
        vm.onResourceGroupChanged = onResourceGroupChanged;
        vm.resourceGroups = [];
        vm.rgButtonText = 'Add New Resource Group';
        vm.saveNewResourceGroup = saveNewResourceGroup;
        vm.toggleNewResourceGroup = toggleNewResourceGroup;

        activate();


        function activate() { }

        function onResourceGroupChanged(item) {
            if (vm.resourceGroupChanged) {
                vm.resourceGroupChanged(item);
            }
        }

        function saveNewResourceGroup() {
            var regex = /^[A-Za-z0-9._\(\)\-]+[A-Za-z0-9\(\)]$/;
            if (!regex.test(vm.newResourceGroupName)) {
                var suggestedName = _.kebabCase(vm.newResourceGroupName.name);
                var msg = 'You entered an invalid resource group name (try this instead: "' + suggestedName + '")';
                toastr.warning(msg, 'Invalid Name');
                return;
            }
            if (vm.newResourceGroupName && vm.location) {
                var newResourceGroup = {
                    resourceGroupName: vm.newResourceGroupName,
                    location: vm.location,
                    subscriptionId: vm.subscriptionId
                };

                if (vm.suppressSaveNew) {
                    onComplete(newResourceGroup);
                } else {
                    ascApi.createResourceGroup(newResourceGroup).then(function (data) {
                        onComplete(newResourceGroup);
                    });
                }
            } else {
                toastr.warning('You must specify both a Resource Group Name and Location!', 'Incomplete');
            }

            function onComplete(newResGroup) {
                var resGroup = { name: newResGroup.resourceGroupName, location: newResGroup.location };
                vm.resourceGroups.push(resGroup);
                vm.selectedResourceGroup = resGroup;
                if (vm.resourceGroupChanged) {
                    vm.resourceGroupChanged(resGroup);
                }
                toggleNewResourceGroup();
            }
        }

        function toggleNewResourceGroup() {
            if (!vm.newRGVisible) {
                ascApi.getStorageProvider(vm.subscriptionId).then(function (data) {
                    var storageAccounts = _.find(data.resourceTypes, { resourceType: 'storageAccounts' });
                    vm.locations = storageAccounts.locations;

                    vm.newRGVisible = !vm.newRGVisible;
                    vm.rgButtonText = vm.newRGVisible ? 'Cancel New Resource Group' : 'Add New Resource Group';
                });
            } else {
                vm.newRGVisible = !vm.newRGVisible;
                vm.rgButtonText = vm.newRGVisible ? 'Cancel New Resource Group' : 'Add New Resource Group';
            }
        }
    }
})();