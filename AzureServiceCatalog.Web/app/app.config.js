(function () {

    angular
        .module('ascApp')
        .config(configApp);

    configApp.$inject =
        ['$stateProvider', '$urlRouterProvider', 'toastrConfig', '$httpProvider', 'adalAuthenticationServiceProvider', 'identityInfo'];

    function configApp($stateProvider, $urlRouterProvider, toastrConfig, $httpProvider, adalProvider, identityInfo) {
        configStates($stateProvider, $urlRouterProvider);

        // Toastr Config
        angular.extend(toastrConfig, {
            newestOnTop: true,
            positionClass: 'toast-bottom-full-width',
            closeButton: true
        });


        // ADAL Config
        //console.log('**identityInfo before ADAL', identityInfo);
        //adalProvider.init(
        //    {
        //        //TODO: remove hard-coded clientId and tenant
        //        clientId: identityInfo.clientId,
        //        tenant: 'stevemic21yahoo.onmicrosoft.com',
        //        //tenant: identityInfo.tenant || 'common',
        //        cacheLocation: 'localStorage' // optional cache location default is sessionStorage
        //    }, $httpProvider); // pass http provider to inject request interceptor to attach tokens
    }

    function configStates($stateProvider, $urlRouterProvider, toastrConfig, $httpProvider, adalProvider) {

        $stateProvider
            .state('dashboard', {
                url: '/',
                templateUrl: '/app/dashboard/dashboard.html',
                controller: 'DashboardCtrl',
                controllerAs: 'vm',
                requireADLogin: true,
                resolve: {
                    initialData: ['identityInfo', 'ascApi', function (identityInfo, ascApi) {
                        if (identityInfo.isAuthenticated) {
                            return ascApi.getTemplates();
                        }
                    }]
                }
            })
            .state('blueprints-home', {
                url: '/blueprints-home',
                templateUrl: '/app/home/blueprints-home.html',
                controller: 'BlueprintsHomeCtrl',
                controllerAs: 'vm',
                requireADLogin: true,
                resolve: {
                    initialData: ['identityInfo', 'ascApi', function (identityInfo, ascApi) {
                        if (identityInfo.isAuthenticated) {
                            return ascApi.getEnrolledSubscriptions();
                        }
                    }]
                }
            })
            .state('home', {
                url: '/home',
                templateUrl: '/app/home/home.html',
                controller: 'HomeCtrl',
                controllerAs: 'vm',
                requireADLogin: true,
                resolve: {
                    initialData: ['identityInfo', 'ascApi', function (identityInfo, ascApi) {
                        if (identityInfo.isAuthenticated) {
                            return ascApi.getTemplates();
                        }
                    }]
                }
            })
            .state('activation-login', {
                url: '/activation-login',
                templateUrl: '/app/identity/activation-login.html',
                controller: 'ActivationLoginCtrl',
                controllerAs: 'vm'
            })
            .state('login', {
                url: '/login',
                templateUrl: '/app/identity/login.html',
                controller: 'LoginCtrl',
                controllerAs: 'vm'
            })
            .state('activation', {
                url: '/activation',
                templateUrl: 'app/identity/activation.html',
                controller: 'ActivationCtrl',
                controllerAs: 'vm'
            })
            .state('deactivate-tenant', {
                url: '/deactivate-tenant',
                templateUrl: 'app/identity/deactivate-tenant.html',
                controller: 'DeactivateTenantCtrl',
                controllerAs: 'vm',
                adminPermissionRequired: true
            })
            .state('product-list', {
                url: '/manage-products/product-list',
                templateUrl: 'app/manage-products/product-list.html',
                controller: 'ProductListCtrl',
                controllerAs: 'vm',
                requireADLogin: true,
                createPermissionRequired: true,
                resolve: {
                    initialData: ['ascApi', function (ascApi) {
                        return ascApi.getTemplates();
                    }]
                }
            })
            .state('edit-product', {
                url: '/manage-products/edit-product/:id',
                templateUrl: 'app/manage-products/edit-product.html',
                controller: 'EditProductCtrl',
                controllerAs: 'vm',
                title: 'Product Details',
                createPermissionRequired: true,
                resolve: {
                    initialData: ['$stateParams', 'ascApi', function ($stateParams, ascApi) {
                        if ($stateParams.id) {
                            return ascApi.getTemplate($stateParams.id);
                        }
                    }]
                }
            })
            .state('add-product-designer', {
                url: '/manage-products/add-product-designer',
                templateUrl: 'app/manage-products/add-product-designer.html',
                controller: 'AddProductDesignerCtrl',
                controllerAs: 'vm',
                createPermissionRequired: true
            })
            .state('product-details', {
                url: '/productdetails/:id',
                params: {
                    product: {
                        squash: true
                    }
                },
                templateUrl: 'app/product-details/product-details.html',
                controller: 'ProductDetailsCtrl',
                controllerAs: 'vm',
                resolve: {
                    initialData: ['ascApi', '$stateParams', function (ascApi, $stateParams) {
                        if ($stateParams.product) {
                            return $stateParams.product;
                        } else if ($stateParams.id) {
                            return ascApi.getTemplate($stateParams.id);
                        }
                    }]
                }
            })
            .state('product-deployment', {
                url: '/productdeployment/:id',
                templateUrl: 'app/product-deployment/product-deployment.html',
                controller: 'ProductDeploymentCtrl',
                controllerAs: 'vm',
                //deployPermissionRequired: true,
                resolve: {
                    initialData: ['$stateParams', 'productDeploymentInitialDataService',
                        function ($stateParams, productDeploymentInitialDataService) {
                            return productDeploymentInitialDataService.getData($stateParams.id);
                        }]
                }
            })
            .state('resource-groups', {
                url: '/resource-groups',
                templateUrl: 'app/resource-groups/resource-groups.html',
                controller: 'ResourceGroupsCtrl',
                controllerAs: 'vm',
                //deployPermissionRequired: true,
                resolve: {
                    initialData: ['ascApi', function (ascApi) {
                        return ascApi.getEnrolledSubscriptions();
                    }]
                }
            })
            .state('deployments', {
                url: '/deployments',
                templateUrl: 'app/deployments/deployments.html',
                controller: 'DeploymentsCtrl',
                controllerAs: 'vm',
                //deployPermissionRequired: true,
                resolve: {
                    initialData: ['ascApi', function (ascApi) {
                        return ascApi.getEnrolledSubscriptions();
                    }]
                }
            })
            .state('deployment', {
                url: '/deployment/:subscriptionId/:correlationId',
                templateUrl: 'app/deployments/deployment.html',
                controller: 'DeploymentCtrl',
                controllerAs: 'vm',
                //deployPermissionRequired: true,
                resolve: {
                    initialData: ['$stateParams', 'ascApi', function ($stateParams, ascApi) {
                        return ascApi.getAuditLogs($stateParams.subscriptionId, $stateParams.correlationId);
                    }]
                }
            })
            .state('manage-host-subscription', {
                url: '/manage-host-subscription',
                templateUrl: 'app/identity/manage-host-subscription.html',
                controller: 'ManageHostSubscriptionCtrl',
                controllerAs: 'vm',
                adminPermissionRequired: true,
                resolve: {
                    initialData: ['ascApi', function (ascApi) {
                        return ascApi.getSubscriptions();
                    }]
                }
            })
            .state('manage-enrolled-subscriptions', {
                url: '/manage-enrolled-subscriptions',
                templateUrl: 'app/identity/manage-enrolled-subscriptions.html',
                controller: 'ManageEnrolledSubscriptions',
                controllerAs: 'vm',
                adminPermissionRequired: true,
                resolve: {
                    initialData: ['ascApi', function (ascApi) {
                        return ascApi.getSubscriptions();
                    }]
                }
            })
            .state('manage-user-groups', {
                url: '/manage-user-groups',
                templateUrl: 'app/identity/manage-user-groups.html',
                controller: 'ManageUserGroupsCtrl',
                controllerAs: 'vm',
                adminPermissionRequired: true,
                resolve: {
                    initialData: ['ascApi', function (ascApi) {
                        return ascApi.getOrganization();
                    }]
                }
            })
            .state('manage-policy-list', {
                url: '/manage-policy-list',
                templateUrl: 'app/policy/policy-list.html',
                controller: 'PolicyListCtrl',
                controllerAs: 'vm',
                adminPermissionRequired: true,
                resolve: {
                    initialData: ['ascApi', function (ascApi) {
                        return ascApi.getEnrolledSubscriptions();
                    }]
                }
            })
            .state('edit-policy', {
                url: '/edit-policy/:subscriptionId/:id',
                templateUrl: 'app/policy/edit-policy.html',
                controller: 'EditPolicyCtrl',
                controllerAs: 'vm',
                adminPermissionRequired: true,
                resolve: {
                    initialData: ['$stateParams', 'ascApi', function ($stateParams, ascApi) {
                        if ($stateParams.id) {
                            return ascApi.getPolicy($stateParams.subscriptionId, $stateParams.id);
                        }
                    }]
                }
            })
            .state('manage-policy-assignments', {
                url: '/manage-policy-assignments',
                templateUrl: 'app/policy-assignments/policy-assignment-list.html',
                controller: 'PolicyAssignmentListCtrl',
                controllerAs: 'vm',
                adminPermissionRequired: true,
                resolve: {
                    initialData: ['ascApi', function (ascApi) {
                        return ascApi.getEnrolledSubscriptions();
                    }]
                }
            })
            .state('edit-policy-assignment', {
                url: '/edit-policy-assignment/:subscriptionId',
                templateUrl: 'app/policy-assignments/edit-policy-assignment.html',
                controller: 'EditPolicyAssignmentCtrl',
                controllerAs: 'vm',
                adminPermissionRequired: true,
                resolve: {
                    initialData: ['$stateParams', 'editPolicyAssignmentInitialDataService',
                        function ($stateParams, editPolicyAssignmentInitialDataService) {
                            return editPolicyAssignmentInitialDataService.getData($stateParams.subscriptionId);
                        }]
                }
            })
            .state('manage-security', {
                url: '/security',
                templateUrl: 'app/security/security.html',
                controller: 'SecurityCtrl',
                controllerAs: 'vm',
                adminPermissionRequired: true,
                resolve: {
                    initialData: ['securityInitialDataService',
                        function (securityInitialDataService) {
                            return securityInitialDataService.getData();
                        }]
                }
            })
            .state('assign-blueprint', {
                url: '/assign-blueprint/:subscriptionId/:blueprintName',
                templateUrl: 'app/blueprint/assign-blueprint.html',
                controller: 'AssignBlueprintCtrl',
                controllerAs: 'vm',
                resolve: {
                    initialData: ['$stateParams', 'ascApi', function ($stateParams, ascApi) {
                        if ($stateParams.blueprintName) {
                            return ascApi.getBlueprintVersions($stateParams.subscriptionId, $stateParams.blueprintName);
                        }
                    }]
                }
            })
            .state('blueprint-assignments', {
                url: '/blueprint-assignments',
                templateUrl: 'app/blueprint/blueprint-assignments.html',
                controller: 'BlueprintAssignmentsCtrl',
                controllerAs: 'vm',
                resolve: {
                    initialData: ['$stateParams', 'ascApi', function ($stateParams, ascApi) {
                        return [];
                    }]
                }
            });

        $urlRouterProvider.otherwise('/');
    }

})();