(function () {
    'use strict';

    angular.module('ascApp').controller('AddProductDesignerCtrl', AddProductDesignerCtrl);

    AddProductDesignerCtrl.$inject =
        ['$timeout', '$state', 'ascApi', 'uuid', 'appSpinner', 'dialogsService', '$window', 'toastr'];

    /* @ngInject */
    function AddProductDesignerCtrl($timeout, $state, ascApi, uuid, appSpinner, dialogs, $window, toastr) {
        /* jshint validthis: true */
        var vm = this;

        vm.clear = clear;
        vm.dropped = dropped;
        vm.helpVisible = true;
        vm.helpDismissed = helpDismissed;
        //vm.productListService = productListService;
        vm.resources = [];
        vm.canSave = canSave;
        vm.dialogs = dialogs;
        vm.product = {
            name: null,
            templateData: null,
            description: null
        };

        vm.resourceTypes = [
            { type: 'azure-web-app', img: 'azure-web-app.png', text: 'Web App' },
            { type: 'azure-sql-database', img: 'azure-sql-database.png', text: 'SQL Database' },
            { type: 'azure-redis-cache', img: 'azure-cache-redis.png', text: 'Redis Cache' },
            { type: 'storage-account', img: 'azure-storage-account.png', text: 'Storage Account' },
            { type: 'azure-windows-vm', img: 'azure-vm.png', text: 'Virtual Machine' },
            { type: 'auto-scale-settings', img: 'azure-autoscale-settings.png', text: 'Autoscale Settings' },
            { type: 'app-insights-components', img: 'azure-app-insights.png', text: 'App Insights' },
            { type: 'app-insights-alert-rule-cpu-high', img: 'azure-app-insights.png', text: 'Alert - CPU High' },
            { type: 'app-insights-alert-rule-server-errors', img: 'azure-app-insights.png', text: 'Alert - Server Errors' },
            //The new ones we are adding go here
            //Didn't add: Operational Insights, HDInsight, Management Services
            //{ type: 'azure-cloud-services', img: 'rubber-duck.png', text: 'Cloud Services' },
            //{ type: 'azure-media-services', img: 'rubber-duck.png', text: 'Media' },
            //{ type: 'azure-service-bus', img: 'rubber-duck.png', text: 'Service Bus' },
            //{ type: 'azure-biztalk', img: 'rubber-duck.png', text: 'Biztalk' },
            //{ type: 'azure-recovery', img: 'rubber-duck.png', text: 'Recovery' },
            //{ type: 'azure-cdn', img: 'rubber-duck.png', text: 'CDN' },
            //{ type: 'azure-automation', img: 'rubber-duck.png', text: 'Automation' },
            //{ type: 'azure-scheduler', img: 'rubber-duck.png', text: 'Scheduler' },
            //{ type: 'azure-api-management', img: 'rubber-duck.png', text: 'API Management' },
            //{ type: 'azure-servic', img: 'rubber-duck.png', text: 'Service Bus' },
            //{ type: 'azure-machine-learning', img: 'rubber-duck.png', text: 'Machine Learning' },
            //{ type: 'azure-stream-analytics', img: 'rubber-duck.png', text: 'Stream Analytics' },
            //{ type: 'azure-networks', img: 'rubber-duck.png', text: 'Networks' },
            //{ type: 'azure-traffic-manager', img: 'rubber-duck.png', text: 'Traffic Manager' },
            //{ type: 'azure-remote-app', img: 'rubber-duck.png', text: 'Remote App' },
            //{ type: 'azure-active-dir', img: 'rubber-duck.png', text: 'Active Directory' }
        ];
        vm.save = save;

        activate();

        ////////////////

        function activate() {
            vm.helpVisible = !$window.localStorage.getItem('productDesignerHelpDismissed');
        }

        function helpDismissed() {
            $window.localStorage.setItem('productDesignerHelpDismissed', true);
            vm.helpVisible = false;
        }

        function clear() {
            vm.resources = [];
        }

        function canSave() {
            return vm.resources.length > 0 && vm.product.name && vm.product.name.length > 0;
        }

        function dropped(dragInfo) {
            $timeout(function () {
                var resourceType = _.find(vm.resourceTypes, { 'type': dragInfo });
                var resourceInstance = angular.copy(resourceType);
                resourceInstance.id = uuid.new();
                vm.resources.push(resourceInstance);
            });
        }

        function save() {
            var templateNames = _.map(vm.resources, 'type');

            appSpinner.showSpinner('Saving Template...');
            ascApi.getAllBaseTempates(templateNames).then(function (data) {

                var allParams = {};
                _.forEach(data, function (item) {
                    _.forIn(item.parameters, function (value, key) {
                        allParams[key] = value;
                    });
                });

                var allVars = {};
                _.forEach(data, function (item) {
                    _.forIn(item.variables, function (value, key) {
                        allVars[key] = value;
                    });
                });

                var allResources = _.chain(data).map('resources').flatten().value();

                var combinedTemplate = {
                    $schema: data[0].$schema,
                    contentVersion: data[0].contentVersion,
                    parameters: allParams,
                    variables: allVars,
                    resources: allResources
                };
                if (vm.origProduct && vm.origProduct.rowKey === vm.product.rowKey && vm.product.name !== vm.origProduct.name) {
                    vm.product.rowKey = null;
                }
                var productToSave = vm.product;
                productToSave.templateData = JSON.stringify(combinedTemplate, null, '\t');
                ascApi.saveTemplate(productToSave).then(function (data) {
                    appSpinner.hideSpinner();
                    toastr.success('The new product, ' + productToSave.name + ', was saved successfully.', 'Save Successful');
                    vm.product = data;
                    vm.origProduct = data;
                });
            });
        }
    }
})();