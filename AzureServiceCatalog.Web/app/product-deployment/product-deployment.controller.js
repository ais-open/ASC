(function () {
    'use strict';

    angular.module('ascApp').controller('ProductDeploymentCtrl', ProductDeploymentCtrl);

    ProductDeploymentCtrl.$inject = ['$state', '$uibModal', 'initialData', 'ascApi', 'dialogsService', 'taskMgr', 'toastr'];

    /* @ngInject */
    function ProductDeploymentCtrl($state, $uibModal, initialData, ascApi, dialogs, taskMgr, toastr) {
        /* jshint validthis: true */
        var vm = this;

        vm.addNewRG = addNewRG;
        vm.deploy = deploy;
        vm.lodash = _;
        vm.newRGVisible = false;
        vm.parameters = [];
        vm.policyLookup = {};
        vm.resourceGroups = [];
        vm.resourceGroupChanged = resourceGroupChanged;
        vm.rgButtonText = 'Add New Resource Group';
        vm.saveNewResourceGroup = saveNewResourceGroup;
        vm.subscriptionChanged = subscriptionChanged;
        vm.subscriptions = initialData.subscriptions;
        vm.templateData = JSON.parse(initialData.product.templateData);
        vm.toggleNewResourceGroup = toggleNewResourceGroup;
        vm.product = initialData.product;

        activate();

        ////////////////


        function activate() {
            _.forIn(vm.templateData.parameters, function (value, key) {
                //If value is JSON object we need to convert it into JSON string so as to display it on UI.
                var itemValue = value.defaultValue;
                if (value.type.toLowerCase() === "array" || value.type.toLowerCase() === "object" || value.type.toLowerCase() === "secureobject") {
                    itemValue = JSON.stringify(value.defaultValue);
                }
                vm.parameters.push({ name: key, info: value, value: itemValue });
            });
        }

        function addNewRG(){
            var rgDialog = $uibModal.open({
                templateUrl: '/app/product-deployment/new-resource-group-modal.html',
                controller: 'NewResourceGroupCtrl',
                controllerAs: 'vm',
                resolve: {
                    initialData: ['newResourceGroupInitialDataService', function (newResourceGroupInitialDataService) {
                        return newResourceGroupInitialDataService.getData(vm.selectedSubscription.id);
                    }],
                    subscriptionId: function () {
                        return vm.selectedSubscription.id;
                    }
                },
                size: 'lg'
            });

            rgDialog.result.then(function (newResourceGroup) {
                var resGroup = { name: newResourceGroup.resourceGroupName };
                vm.resourceGroups.push(resGroup);
                vm.selectedResourceGroup = resGroup;
                resourceGroupChanged(resGroup);
            });
        }

        function deploy() {
            try {
                var params = {};
                _.forEach(vm.parameters, function (item) {

                    //We need to stringify the default value because we are changing the value to string in previous obj to display on UI
                    var defaultValue = item.info.defaultValue;
                    if (item.info.type.toLowerCase() === "array" || item.info.type.toLowerCase() === "object" || item.info.type.toLowerCase() === "secureobject") {
                        defaultValue = JSON.stringify(defaultValue);
                    }

                    //If template already contains default value and we are not changing the value 
                    //then we will keep default value and not send the value of parameter in parameters object
                    if (defaultValue != item.value) {
                        var itemValue = item.value;
                        if (item.info.type.toLowerCase() === "array" || item.info.type.toLowerCase() === "object" || item.info.type.toLowerCase() === "secureobject") {
                            itemValue = JSON.parse(item.value);
                        }
                        if (item.info.type.toLowerCase() === "int") {
                            itemValue = parseInt(item.value, 10);
                        }
                        if (item.info.type.toLowerCase() === "bool") {
                            if (itemValue.toLowerCase() === "true") {
                                itemValue = true;
                            } else {
                                itemValue = false;
                            }
                        }
                        params[item.name] = { value: itemValue };
                    }
                });

                var deployment = {
                    resourceGroupName: vm.selectedResourceGroup.name,
                    deploymentName: 'Deploy-' + moment().format('HH.mm.ss'),
                    templateName: initialData.product.rowKey,
                    subscriptionId: vm.selectedSubscription.rowKey,
                    parameters: JSON.stringify(params)
                };

                console.log('***deployment', deployment);
                ascApi.validateDeployment(deployment).then(function (data) {
                    if (data.isValid) {
                        ascApi.createDeployment(deployment).then(function (data) {
                            console.log('Deployment created.', data);
                            vm.operationId = data.requestId;
                            taskMgr.addDeployTask(deployment.resourceGroupName, deployment.deploymentName, deployment.subscriptionId);
                            //$state.go('user');
                        });
                    } else {
                        toastr.error(data.error.message, 'Validation Errors - Deploy Not Executed');
                    }
                });
            }
            catch {
                toastr.error('Please provide correct format value of parameters according to the Parameter Type.', 'Parameters Validation Errors');
            }
        }

        function resourceGroupChanged() {
            var policyNames = _.chain(vm.parameters)
                               .filter(function (param) {
                                   return param.info.metadata.policy;
                               })
                               .map('info.metadata.policy').value();

            ascApi.getPolicies(vm.selectedSubscription.id).then(function (data) {
                _.forEach(data, function (policyDefinition) {
                    if (_.includes(policyNames, policyDefinition.policy.name)) {
                        var fullJsonPath = 'properties.policyRule.' + policyDefinition.policyLookupPath;
                        vm.policyLookup[policyDefinition.policy.name] = _.get(policyDefinition.policy, fullJsonPath);
                    }
                });
            });
        }

        function saveNewResourceGroup() {

            if (vm.newResourceGroupName && vm.location) {
                var newResourceGroup = {
                    resourceGroupName: vm.newResourceGroupName,
                    location: vm.location,
                    subscriptionId: vm.selectedSubscription.rowKey
                };

                ascApi.createResourceGroup(newResourceGroup).then(function (data) {
                    var resGroup = { name: data.resourceGroupName };
                    vm.resourceGroups.push(resGroup);
                    vm.selectedResourceGroup = resGroup;
                    resourceGroupChanged(resGroup);
                    toggleNewResourceGroup();
                });
            } else {
                toastr.warning('You must specify both a Resource Group Name and Location!', 'Incomplete');
            }
        }

        function subscriptionChanged(item, model)
        {
            vm.resourceGroups = [];
            vm.selectedResourceGroup = null;
            ascApi.getResourceGroupsBySubscription(item.rowKey).then(
                function (data) {
                    if (data) {
                        vm.resourceGroups = data.value;
                    }
                },
                function (error) {
                    toastr.error(error, 'Error');
                }
            );
        }

        function toggleNewResourceGroup() {
            if (!vm.newRGVisible) {
                ascApi.getStorageProvider(vm.selectedSubscription.rowKey).then(function (data) {
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