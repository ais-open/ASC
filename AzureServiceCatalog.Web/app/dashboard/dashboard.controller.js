(function () {
    'use strict';

    angular.module('ascApp').controller('DashboardCtrl', DashboardCtrl);
    DashboardCtrl.$inject = ['identityInfo'];

    /* @ngInject */
    function DashboardCtrl(identityInfo) {
        /* jshint validthis: true */
        var vm = this;

        //vm.getResourceNameDisplay = getResourceNameDisplay;
        //vm.isDirectoryAuthenticated = identityInfo.isAuthenticated && identityInfo.directoryName;
        vm.isBlueprintFeatureEnabled = identityInfo.isBlueprintFeatureEnabled;
        vm.isPoliciesFeatureEnabled = identityInfo.isPoliciesFeatureEnabled;
        vm.isProductCatalogFeatureEnabled = identityInfo.isProductCatalogFeatureEnabled;
        //vm.products = initialData;
        //vm.resourceDictionary = productListService.resourceDictionary;
        //vm.hasPublishedProducts = false;
        //vm.userDetail = appStorage.getUserDetail();
        vm.isAuthenticated = identityInfo.isAuthenticated;
        //activate();

        //console.log('**identityInfo in DashboardCtrl', identityInfo);

        //function activate() {
        //    if (vm.products) {
        //        vm.products.forEach(function (product) {
        //            product.data = JSON.parse(product.templateData);
        //            if (product.isPublished) {
        //                vm.hasPublishedProducts = true;
        //            }
        //        });
        //    }
        //}

        //function getResourceNameDisplay(resource){
        //    if (!resource.name || _.startsWith(resource.name, '[')) {
        //        return null;
        //    } else {
        //        return ' (' + resource.name + ')';
        //    }
        //}
    }
})();