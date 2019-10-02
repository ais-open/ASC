(function () {
    'use strict';

    angular.module('ascApp').controller('ManageUserGroupsCtrl', ManageUserGroupsCtrl);
    ManageUserGroupsCtrl.$inject = ['initialData', 'ascApi', 'toastr'];

    /* @ngInject */
    function ManageUserGroupsCtrl(initialData, ascApi, toastr) {
        /* jshint validthis: true */
        var vm = this;
        vm.createGroups = [];
        vm.adminGroups = [];
        vm.filterGroups = filterGroups;
        vm.organization = initialData;
        vm.adminGroupChange = adminGroupChange;
        vm.createProductGroupChange = createProductGroupChange;
        vm.adminGroupDetail = [];
        vm.createProductGroupDetail = [];
        vm.save = save;

        activate();

        function activate() {
            vm.createGroups = vm.organization.organizationADGroups;
            vm.adminGroups = vm.organization.organizationADGroups;
        }

        function filterGroups(filter, groupType) {
            if (filter) {
                ascApi.getOrganizationGroups(filter).then(function (data) {
                    vm[groupType] = data;
                });
            }
        }

        function adminGroupChange(item) {
            vm.adminGroupDetail = item.name;
        }

        function createProductGroupChange(item) {
            vm.createProductGroupDetail = item.name;
        }

        function save() {
            if (vm.adminGroupDetail.length !== 0) {
                vm.organization.adminGroupName = vm.adminGroupDetail;
            }

            if (vm.createProductGroupDetail.length !== 0) {
                vm.organization.createProductGroupName = vm.createProductGroupDetail;
            }

            ascApi.saveOrganization(vm.organization).then(function (data) {
                toastr.success('The Organization AD Groups were saved successfully.', 'Save Successful');
            });
        }
    }
})();