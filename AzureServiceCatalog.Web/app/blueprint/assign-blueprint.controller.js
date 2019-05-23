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

        vm.lodash = _;
        vm.parameters = [];
        vm.resourceGroups = [];
        vm.blueprintDefinitionVersion = initialData.name;
        vm.blueprintName = initialData.properties.blueprintName;
        vm.assignmentName = 'Assignment-' + vm.blueprintName;
        vm.targetScope = initialData.properties.targetScope;




        activate();

        function activate() {
            vm.blueprintVersion = initialData;
            var tempArr = vm.blueprintVersion.id.split('/');
            var subscriptionsIndex = tempArr.indexOf('subscriptions');
            vm.subscriptionId = tempArr[subscriptionsIndex + 1];


            console.log('ResourceGroups');
            console.log(initialData.properties.resourceGroups);


            _.forIn(initialData.properties.parameters, function (value, key) {
                vm.parameters.push({ name: key, info: value, value: value.defaultValue });
            });

            _.forIn(initialData.properties.resourceGroups, function (value, key) {
                vm.resourceGroups.push({ name: key, info: value, value: value.name });
            });

            console.log('vm.resourceGroups');
            console.log(vm.resourceGroups);

            console.log('vm.parameters');
            console.log(vm.parameters);
        }
        }

        function assign() {
            var blueprintAssignment = {
                "identity": {
                    "type": "SystemAssigned"
                },
                "location": "eastus",
                "properties": {},
                "resourceGroups": {}
            };

            _.forIn(vm.parameters, function (value, key) {
                vm.resourceGroups.push({ name: key, info: value, value: value.name });
            });

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
)();
