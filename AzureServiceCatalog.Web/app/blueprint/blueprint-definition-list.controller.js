(function () {
    'use strict';

    angular.module('ascApp').controller('BlueprintDefinitionListCtrl', BlueprintDefinitionListCtrl);
    BlueprintDefinitionListCtrl.$inject = ['initialData', 'ascApi'];

    /* @ngInject */
    function BlueprintDefinitionListCtrl(initialData, ascApi) {
        /* jshint validthis: true */
        var vm = this;
        vm.blueprintDefinitions = [];
        vm.selectedSubscription = null;
        vm.getLastModifiedDate = getLastModifiedDate;
        vm.subscriptions = initialData;
        vm.subscriptionChanged = subscriptionChanged;

        activate();

        function activate() {
            console.log('in activate');
            if (vm.subscriptions && vm.subscriptions.length > 0) {
                vm.selectedSubscription = vm.subscriptions[0];
                subscriptionChanged();
            }
        }

        function getLastModifiedDate(blueprintDefinition) {
            var lastModifiedDate = "";
            var status = blueprintDefinition.properties["status"];
            if (status !== undefined) {
                if (status.lastModified !== undefined) {
                    var newDate = new Date(status.lastModified);
                    lastModifiedDate = newDate.toLocaleDateString();
                }
            }
            return lastModifiedDate;
        }

        function subscriptionChanged() {
            ascApi.getBlueprintDefinitions(vm.selectedSubscription.rowKey).then(function (data) {
                console.log(data);
                vm.blueprintDefinitions = data.value;
            });
        }
    }
})();