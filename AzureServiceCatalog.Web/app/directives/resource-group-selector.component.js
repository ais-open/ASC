(function () {
    'use strict';

    angular.module('ascApp').component('resourceGroupSelector', {
        templateUrl: '/app/shared/resource-group-selector.html',
        controllerAs: 'vm',
        controller: 'ResourceGroupSelectorCtrl',
        bindings: {
            resourceGroups: '=',
            subscriptionId: '=',
            selectedResourceGroup: '=',
            suppressSaveNew: '<',
            resourceGroupChanged: '='
        }
    });
})();
