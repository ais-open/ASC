(function () {
    'use strict';

    angular.module('ascApp').controller('DashboardCtrl', DashboardCtrl);
    DashboardCtrl.$inject = ['identityInfo', 'initialData', 'productListService', 'appStorage'];

    /* @ngInject */
    function DashboardCtrl(identityInfo, initialData, productListService, appStorage) {
        /* jshint validthis: true */
        var vm = this;

        vm.getResourceNameDisplay = getResourceNameDisplay;
        vm.isDirectoryAuthenticated = identityInfo.isAuthenticated && identityInfo.directoryName;
        vm.products = initialData;
        vm.resourceDictionary = productListService.resourceDictionary;
        vm.hasPublishedProducts = false;
        vm.userDetail = appStorage.getUserDetail();
        activate();

        //console.log('**identityInfo in DashboardCtrl', identityInfo);

        function activate() {
            if (vm.products) {
                vm.products.forEach(function (product) {
                    product.data = JSON.parse(product.templateData);
                    if (product.isPublished) {
                        vm.hasPublishedProducts = true;
                    }
                });
            }
        }

        function getResourceNameDisplay(resource){
            if (!resource.name || _.startsWith(resource.name, '[')) {
                return null;
            } else {
                return ' (' + resource.name + ')';
            }
        }
    }
})();