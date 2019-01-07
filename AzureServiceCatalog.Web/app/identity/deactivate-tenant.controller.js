(function () {
    'use strict';

    angular.module('ascApp').controller('DeactivateTenantCtrl', DeactivateTenantCtrl);
    DeactivateTenantCtrl.$inject = ['ascApi', 'toastr', 'dialogsService'];

    /* @ngInject */
    function DeactivateTenantCtrl(ascApi, toastr, dialogs) {
        /* jshint validthis: true */
        var vm = this;
        vm.removeTenant = removeTenant;

        activate();

        function activate() { }

        function removeTenant() {
            dialogs.confirm('Are you sure you want to remove this Tenant? This action cannot be undone without manually ' +
                're-adding the tenant to the azure subscription.',
                'Remove Tenant', ['Yes', 'No']).then(function () {
                    return ascApi.deleteOrganization().then(function (response) {
                        //The response is currently always undefined.
                        //TODO: Return some kind of usable response for conditional success toasts.
                        if (response) {
                            toastr.success('This tenant was successfully removed.', 'Removal Successful');
                        }
                    });
            });
        }
    }
})();