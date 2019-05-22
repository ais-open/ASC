(function () {
    'use strict';

    angular.module('ascApp').controller('AssignBlueprintCtrl', AssignBlueprintCtrl);
    AssignBlueprintCtrl.$inject = ['initialData', 'ascApi'];

    /* @ngInject */
    function AssignBlueprintCtrl(initialData, ascApi) {
        /* jshint validthis: true */
        var vm = this;
        vm.assignmentName = "";
        vm.blueprintVersion = null;
        activate();

        function activate() {
            vm.blueprintVersion = initialData;
            var tempArr = vm.blueprintVersion.id.split('/');
            var subscriptionsIndex = tempArr.indexOf('subscriptions');
            vm.subscriptionId = tempArr[subscriptionsIndex + 1];
        }

        function assign() {
            var blueprintAssignment = {  };

            ascApi.assignBlueprint(vm.subscriptionId, vm.assignmentName, blueprintAssignment).then(function (data) {
                if (data.error) {
                    console.log('Error while assigning blueprint!', data);
                    toastr.error('Unexpected error while assigning.', 'Error');
                } else {
                    vm.policy = data.policy;
                    toastr.success('Blueprint assigned successfully.', 'Success');
                }
            });
        }
    }
})();