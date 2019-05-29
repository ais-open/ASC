(function () {
    'use strict';

    var siteMap = [
        { state: 'home', title: 'Home' },
        { state: 'product-list',  title: 'Manage Product Catalog',
            descendants: [
                { state: 'add-product-designer', title: 'Add Product (drag/drop)' },
                { state: 'edit-product', title: 'Product Details' }
            ]
        },
        {
            state: 'product-catalog', title: 'Product Catalog',
            descendants: [
                { state: 'product-details', title: 'Product Details' },
            ]
        },
        //{ state: 'product-details', title: 'Product Details' },
        { state: 'product-deployment', title: 'Product Provisioning' },
        { state: 'resource-groups', title: 'Spend' },
        { state: 'deployments', title: 'Provisioned',
            descendants: [
                { state: 'deployment', title: 'Provision Detail' },
            ]
        },
        { state: 'manage-host-subscription', title: 'Manage Host Subscription' },
        { state: 'manage-enrolled-subscriptions', title: 'Manage Enrolled Subscriptions' },
        { state: 'manage-user-groups', title: 'Manage User Groups' },
        { state: 'manage-blueprint-definition-list', title: 'Manage Blueprint Definition List',
            descendants: [
                { state: 'show-blueprint-versions', title: 'Blueprint Details'},
                { state: 'get-blueprint-version', title: 'Assign Blueprint' },
            ]
        },
        { state: 'manage-assigned-blueprint-list', title: 'Manage Assigned Blueprint List'},
        { state: 'manage-policy-list', title: 'Manage Policy List',
            descendants: [{ state: 'edit-policy', title: 'Policy' }]
        },
        { state: 'manage-policy-assignments', title: 'Manage Policy Assignments',
            descendants: [{ state: 'edit-policy-assignment', title: 'Edit Policy Assignment' }]
        },
        { state: 'manage-security', title: 'Manage Security' },
        { state: 'usage-dashboard', title: 'Usage Dashboard' },
        { state: 'activation', title: 'Enrollment' },
        { state: 'activation-login', title: 'Activation Login' },
        { state: 'deactivate-tenant', title: 'Deactivate Tenant' }
    ];

    angular.module('ascApp')
        .constant('siteMap', siteMap);
})();
