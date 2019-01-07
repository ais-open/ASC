(function () {
    'use strict';

    angular.module('ascApp').factory('productListService', productListService);

    // productListService.$inject = ['ascApi'];

    /* @ngInject */
    function productListService() {
        var resourceDictionary = {};
        //Auto-generated list of resources using a powershell script, iterating over all possible azure resources
        //Ask Adam for a copy of the powershell script if needed
        //TODO: Fill in the image as new resources are encountered.
        /* jshint ignore:start */
        resourceDictionary['Microsoft.Authorization/roleAssignments'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Authorization/roleDefinitions'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Authorization/classicAdministrators'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Authorization/permissions'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Authorization/locks'] = { name: 'Resource Locks', imageSrc: 'azure-multi-factor-authentication.png' };
        resourceDictionary['Microsoft.Authorization/operations'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Authorization/policyDefinitions'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Authorization/policyAssignments'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Authorization/providerOperations'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Batch/batchAccounts'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Batch/operations'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Batch/locations'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Batch/locations/quotas'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Cache/Redis'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Cache/checkNameAvailability'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Cache/operations'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Cache/RedisConfigDefinition'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Cache/Redis/metricDefinitions'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Cache/Redis/diagnosticSettings'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Cdn/profiles'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Cdn/profiles/endpoints'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Cdn/profiles/endpoints/origins'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Cdn/profiles/endpoints/customdomains'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Cdn/operationresults'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Cdn/operationresults/profileresults'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Cdn/operationresults/profileresults/endpointresults'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Cdn/operationresults/profileresults/endpointresults/originresults'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Cdn/operationresults/profileresults/endpointresults/customdomainresults'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Cdn/checkNameAvailability'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Cdn/operations'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Cdn/edgenodes'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.ClassicStorage/storageAccounts'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.ClassicStorage/quotas'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.ClassicStorage/checkStorageAccountAvailability'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.ClassicStorage/storageAccounts/services'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.ClassicStorage/storageAccounts/services/diagnosticSettings'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.ClassicStorage/storageAccounts/services/metricDefinitions'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.ClassicStorage/storageAccounts/services/metrics'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.ClassicStorage/storageAccounts/metricDefinitions'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.ClassicStorage/storageAccounts/metrics'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.ClassicStorage/capabilities'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.ClassicStorage/disks'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.ClassicStorage/images'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.ClassicStorage/osImages'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.ClassicStorage/operations'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Compute/availabilitySets'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Compute/virtualMachines'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Compute/virtualMachines/extensions'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Compute/virtualMachines/diagnosticSettings'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Compute/virtualMachines/metricDefinitions'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Compute/virtualMachineScaleSets'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Compute/virtualMachineScaleSets/extensions'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Compute/virtualMachineScaleSets/virtualMachines'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Compute/virtualMachineScaleSets/networkInterfaces'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Compute/virtualMachineScaleSets/virtualMachines/networkInterfaces'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Compute/locations'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Compute/locations/operations'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Compute/locations/vmSizes'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Compute/locations/usages'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Compute/locations/publishers'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Compute/operations'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.DevTestLab/labs'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.DevTestLab/environments'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.DevTestLab/operations'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.DevTestLab/locations'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.DevTestLab/locations/operations'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.DocumentDB/databaseAccounts'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.DocumentDB/databaseAccountNames'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.DocumentDB/operations'] = { name: '', imageSrc: '' };
        resourceDictionary['microsoft.insights/components'] = { name: '', imageSrc: '' };
        resourceDictionary['microsoft.insights/webtests'] = { name: '', imageSrc: '' };
        resourceDictionary['microsoft.insights/queries'] = { name: '', imageSrc: '' };
        resourceDictionary['microsoft.insights/alertrules'] = { name: '', imageSrc: 'azure-alert.png' };
        resourceDictionary['microsoft.insights/autoscalesettings'] = { name: '', imageSrc: 'azure-autoscale-settings.png' };
        resourceDictionary['microsoft.insights/eventtypes'] = { name: '', imageSrc: '' };
        resourceDictionary['microsoft.insights/locations'] = { name: '', imageSrc: '' };
        resourceDictionary['microsoft.insights/locations/operationResults'] = { name: '', imageSrc: '' };
        resourceDictionary['microsoft.insights/operations'] = { name: '', imageSrc: '' };
        resourceDictionary['microsoft.insights/automatedExportSettings'] = { name: '', imageSrc: 'azure-automation.png' };
        resourceDictionary['microsoft.insights/diagnosticSettings'] = { name: '', imageSrc: '' };
        resourceDictionary['microsoft.insights/metricDefinitions'] = { name: '', imageSrc: '' };
        resourceDictionary['microsoft.insights/logDefinitions'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.KeyVault/vaults'] = { name: '', imageSrc: 'azure-key-vault.png' };
        resourceDictionary['Microsoft.KeyVault/vaults/secrets'] = { name: '', imageSrc: 'azure-key-vault.png' };
        resourceDictionary['Microsoft.KeyVault/operations'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Logic/workflows'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Logic/operations'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Network'] = { name: '', imageSrc: 'azure-virtual-network.png' };
        resourceDictionary['Microsoft.Network/virtualNetworks'] = { name: '', imageSrc: 'azure-virtual-network.png' };
        resourceDictionary['Microsoft.Network/publicIPAddresses'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Network/networkInterfaces'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Network/loadBalancers'] = { name: '', imageSrc: 'azure-load-balancer.png' };
        resourceDictionary['Microsoft.Network/networkSecurityGroups'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Network/routeTables'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Network/virtualNetworkGateways'] = { name: '', imageSrc: 'azure-vpn-gateway.png' };
        resourceDictionary['Microsoft.Network/localNetworkGateways'] = { name: '', imageSrc: 'azure-vpn-gateway.png' };
        resourceDictionary['Microsoft.Network/connections'] = { name: 'Network Connection', imageSrc: 'azure-connection.png' };
        resourceDictionary['Microsoft.Network/applicationGateways'] = { name: '', imageSrc: 'azure-vpn-gateway.png' };
        resourceDictionary['Microsoft.Network/locations'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Network/locations/operations'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Network/locations/operationResults'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Network/locations/CheckDnsNameAvailability'] = { name: '', imageSrc: 'azure-dns.png' };
        resourceDictionary['Microsoft.Network/locations/usages'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Network/operations'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Network/expressRouteCircuits'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Network/expressRouteServiceProviders'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Network/dnszones'] = { name: '', imageSrc: 'azure-dns.png' };
        resourceDictionary['Microsoft.Network/dnszones/A'] = { name: '', imageSrc: 'azure-dns.png' };
        resourceDictionary['Microsoft.Network/dnszones/AAAA'] = { name: '', imageSrc: 'azure-dns.png' };
        resourceDictionary['Microsoft.Network/dnszones/CNAME'] = { name: '', imageSrc: 'azure-dns.png' };
        resourceDictionary['Microsoft.Network/dnszones/PTR'] = { name: '', imageSrc: 'azure-dns.png' };
        resourceDictionary['Microsoft.Network/dnszones/MX'] = { name: '', imageSrc: 'azure-dns.png' };
        resourceDictionary['Microsoft.Network/dnszones/TXT'] = { name: '', imageSrc: 'azure-dns.png' };
        resourceDictionary['Microsoft.Network/dnszones/SRV'] = { name: '', imageSrc: 'azure-dns.png' };
        resourceDictionary['Microsoft.Network/trafficmanagerprofiles'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Network/checkTrafficManagerNameAvailability'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.OperationalInsights'] = { name: '', imageSrc: 'azure-operational-insights.png' };
        resourceDictionary['Microsoft.OperationalInsights/workspaces'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.OperationalInsights/storageInsightConfigs'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.OperationalInsights/linkTargets'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.OperationalInsights/operations'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Sql'] = { name: '', imageSrc: 'azure-sql-database-generic.png' };
        resourceDictionary['Microsoft.Sql/operations'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Sql/locations'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Sql/locations/capabilities'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Sql/checkNameAvailability'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Sql/servers'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Sql/servers/databases'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Sql/servers/serviceObjectives'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Sql/servers/administrators'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Sql/servers/administratorOperationResults'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Sql/servers/restorableDroppedDatabases'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Sql/servers/recoverableDatabases'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Sql/servers/import'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Sql/servers/importExportOperationResults'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Sql/servers/operationResults'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Sql/servers/firewallrules'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Sql/servers/databaseSecurityPolicies'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Sql/servers/databaseSecurityMetrics'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Sql/servers/auditingPolicies'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Sql/servers/recommendedElasticPools'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Sql/servers/databases/auditingPolicies'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Sql/servers/databases/connectionPolicies'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Sql/servers/databases/securityMetrics'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Sql/servers/databases/dataMaskingPolicies'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Sql/servers/databases/dataMaskingPolicies/rules'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Sql/servers/databases/securityAlertPolicies'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Sql/servers/securityAlertPolicies'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Sql/servers/databases/auditingSettings'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Sql/servers/auditingSettings'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Sql/servers/resourcepools'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Sql/servers/elasticpools'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Sql/servers/usages'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Sql/servers/databases/metricDefinitions'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Sql/servers/databases/metrics'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Sql/servers/elasticpools/metrics'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Sql/servers/elasticpools/metricdefinitions'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Sql/servers/databases/topQueries'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Sql/servers/databases/topQueries/queryText'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Sql/servers/advisors'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Sql/servers/elasticPoolEstimates'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Storage'] = { name: '', imageSrc: 'azure-storage-account.png' };
        resourceDictionary['Microsoft.Storage/storageAccounts'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Storage/operations'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Storage/usages'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Storage/checkNameAvailability'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Storage/storageAccounts/services'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Storage/storageAccounts/services/metricDefinitions'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Web/sites/extensions'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Web/sites/slots/extensions'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Web/sites/instances'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Web/sites/slots/instances'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Web/sites/instances/extensions'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Web/sites/slots/instances/extensions'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Web/publishingUsers'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Web/ishostnameavailable'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Web/sourceControls'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Web/availableStacks'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Web/listSitesAssignedToHostName'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Web/sites/hostNameBindings'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Web/sites/slots/hostNameBindings'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Web/operations'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Web/certificates'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Web/serverFarms'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Web/sites'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Web/sites/slots'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Web/runtimes'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Web/sites/metrics'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Web/sites/metricDefinitions'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Web/sites/slots/metrics'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Web/sites/slots/metricDefinitions'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Web/serverFarms/metrics'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Web/serverFarms/metricDefinitions'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Web/sites/recommendations'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Web/recommendations'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Web/georegions'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Web/sites/premieraddons'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Web/hostingEnvironments'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Web/hostingEnvironments/multiRolePools'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Web/hostingEnvironments/workerPools'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Web/hostingEnvironments/metrics'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Web/hostingEnvironments/metricDefinitions'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Web/hostingEnvironments/multiRolePools/metrics'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Web/hostingEnvironments/multiRolePools/metricDefinitions'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Web/hostingEnvironments/workerPools/metrics'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Web/hostingEnvironments/workerPools/metricDefinitions'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Web/hostingEnvironments/multiRolePools/instances'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Web/hostingEnvironments/multiRolePools/instances/metrics'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Web/hostingEnvironments/multiRolePools/instances/metricDefinitions'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Web/hostingEnvironments/workerPools/instances'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Web/hostingEnvironments/workerPools/instances/metrics'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Web/hostingEnvironments/workerPools/instances/metricDefinitions'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Web/managedHostingEnvironments'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Web/deploymentLocations'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Web/ishostingenvironmentnameavailable'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Web/classicMobileServices'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.ADHybridHealthService/services'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.ADHybridHealthService/addsservices'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.ADHybridHealthService/configuration'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.ADHybridHealthService/operations'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.ADHybridHealthService/agents'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.ADHybridHealthService/aadsupportcases'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.ADHybridHealthService/reports'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.ADHybridHealthService/servicehealthmetrics'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.ADHybridHealthService/logs'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.ADHybridHealthService/anonymousapiusers'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Features/features'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Features/providers'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Resources/subscriptions'] = { name: '', imageSrc: 'azure-subscription.png' };
        resourceDictionary['Microsoft.Resources/subscriptions/providers'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Resources/subscriptions/operationresults'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Resources/resourceGroups'] = { name: '', imageSrc: 'azure-resource-group.png' };
        resourceDictionary['Microsoft.Resources/subscriptions/locations'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Resources/subscriptions/tagnames'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Resources/links'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Resources/deployments'] = { name: 'Deployment', imageSrc: '' };
        resourceDictionary['Microsoft.Resources/deployments/operations'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Resources/operations'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Scheduler/jobcollections'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Scheduler/operations'] = { name: '', imageSrc: '' };
        resourceDictionary['Microsoft.Scheduler/flows'] = { name: '', imageSrc: '' };
        resourceDictionary['microsoft.support/operations'] = { name: '', imageSrc: '' };
        resourceDictionary['microsoft.support/supporttickets'] = { name: '', imageSrc: '' };
        resourceDictionary['microsoft.visualstudio/account'] = { name: '', imageSrc: '' };
        resourceDictionary['microsoft.visualstudio/account/project'] = { name: '', imageSrc: '' };

        //Prefilled original resource dictionaries
        resourceDictionary['Microsoft.Web/sites'] = { name: 'Web App', imageSrc: 'azure-web-app.png' };
        resourceDictionary['Microsoft.Sql/servers'] = { name: 'SQL Server', imageSrc: 'azure-sql-database.png' };
        resourceDictionary['Microsoft.Web/serverfarms'] = { name: 'App Service Plan', imageSrc: 'azure-app-service.png' };
        resourceDictionary['microsoft.insights/autoscalesettings'] = { name: 'AutoScale Settings', imageSrc: 'azure-autoscale-settings.png' };
        resourceDictionary['microsoft.insights/alertrules'] = { name: 'Alert Rules', imageSrc: 'azure-alert.png' };
        resourceDictionary['microsoft.insights/components'] = { name: 'Component Rules', imageSrc: 'azure-startup-task.png' };
        resourceDictionary['Microsoft.ClassicStorage/StorageAccounts'] = { name: 'Storage Accounts', imageSrc: 'azure-storage-account.png' };
        resourceDictionary['Microsoft.Storage/storageAccounts'] = { name: 'Storage Accounts', imageSrc: 'azure-storage-account.png' };
        resourceDictionary['Microsoft.Compute/availabilitySets'] = { name: 'Availability Sets', imageSrc: 'azure-automatic-load-balancer.png' };
        resourceDictionary['Microsoft.Network/publicIPAddresses'] = { name: 'IP Addresses', imageSrc: 'azure-virtual-network.png' };
        resourceDictionary['Microsoft.Network/loadBalancers'] = { name: 'Network Load Balancers', imageSrc: 'azure-load-balancer.png' };
        resourceDictionary['Microsoft.Network/virtualNetworks'] = { name: 'Virtual Networks', imageSrc: 'azure-virtual-network.png' };
        resourceDictionary['Microsoft.WinFab/clusters'] = { name: 'Clusters', imageSrc: 'azure-web-roles.png' };
        resourceDictionary['Microsoft.Network/publicIPAddresses'] = { name: 'Public IP Addresses', imageSrc: 'azure-virtual-network.png' };
        resourceDictionary['Microsoft.Network/networkInterfaces'] = { name: 'Network Interfaces', imageSrc: 'azure-virtual-network.png' };
        resourceDictionary['Microsoft.Compute/virtualMachines/extensions'] = { name: 'VM Extensions', imageSrc: 'azure-virtual-machine.png' };
        resourceDictionary['Microsoft.Compute/virtualMachines'] = { name: 'Virtual Machine', imageSrc: 'azure-vm.png' };
        resourceDictionary['databases'] = { name: 'SQL Database', imageSrc: 'azure-sql-database-generic.png' };
        resourceDictionary['firewallrules'] = { name: 'Firewall Rules', imageSrc: 'azure-service-bus.png' }; //Not sure what image to select for firewall.
        resourceDictionary['Microsoft.Cache/Redis'] = { name: 'Redis Cache', imageSrc: 'azure-redis-cache.png' };
        resourceDictionary['unknown'] = { name: null, imageSrc: 'azure-cloud-service.png' };
        /* jshint ignore:end */
        var service = {
            resourceDictionary: resourceDictionary,
            createActionList: createActionList,
            createProduct: createProduct,
            createPropertyArray: createPropertyArray,
            getParameterKey: getParameterKey,
            parseResourceTypeName: parseResourceTypeName,
            determineResourceImage: determineResourceImage
        };
        return service;

        function getParameterKey(name) {
            if (_.startsWith(name, '[parameters')) {

                var firstIndex = _.indexOf(name, '\'') + 1;
                var lastIndex = _.lastIndexOf(name, '\'');
                name = name.substring(firstIndex, lastIndex);
                return name;
            }
            return name;
        }

        function createPropertyArray(properties) {
            var props = [];
            _.forOwn(properties, function (value, key) {
                if (key !== 'condition' && key !== 'action') {
                    props.push({
                        name: key,
                        value: value
                    });
                }

            });
            return props;
        }

        function createProduct(initialData) {
            var list = [initialData];
            var products = createActionList(list);
            return products[0];
        }

        function createActionList(initialData) {
            var list = _.map(initialData, function (item) {
                return {
                    name: item.name,
                    id: item.rowKey,
                    data: JSON.parse(item.templateData),
                    description: item.description,
                    productPrice: item.productPrice,
                    productImagePath: item.productImagePath
                };
            });
            _.forEach(list, function (product) {
                _.forEach(product.data.resources, function (resource) {
                    var propertyList = createPropertyArray(resource.properties);
                    resource.propertyList = propertyList;
                    _.forEach(resource.resources, function (res) {
                        var resPropertyList = createPropertyArray(res.properties);
                        res.resPropertyList = resPropertyList;
                    });
                });
            });
            return list;
        }

        function determineResourceImage(resource) {
            var resourceType = resource.type;
            return determineResourceImageFromResourceType(resourceType);
        }

        function determineResourceImageFromResourceType(resourceType) {
            var resourceImage = null;
            if (resourceType && resourceDictionary[resourceType]) {
                if (resourceDictionary[resourceType].imageSrc) {
                    resourceImage = resourceDictionary[resourceType].imageSrc;
                } else {
                    //Try to parse the resource type's parent.
                    var resourceTypeTree = resourceType.split('/');
                    if (resourceTypeTree.length > 1) {
                        var parentResourceType = resourceType
                            .substr(0, resourceType.length - resourceTypeTree[resourceTypeTree.length - 1].length - 1);
                        if (parentResourceType && parentResourceType.length > 0) {
                            return determineResourceImageFromResourceType(parentResourceType);
                        }
                    }
                }
            }
            if (!resourceImage) {
                resourceImage = 'azure-cloud-service.png';
            }
            return '/Content/images/' + resourceImage;
        }

        function parseResourceTypeName(resource) {
            if (resource && resource.type) {
                //If an entry exists in the dictionary, return it.
                if (resourceDictionary[resource.type] && resourceDictionary[resource.type].name) {
                    return resourceDictionary[resource.type].name;
                }
                //Otherwise try to parse a friendly display name from the resource.type property.
                //Parse the resource type
                //Resources from Microsoft often are formatted as Microsoft.ResourceParentType/ResourceType
                var splitResourceString = resource.type.split('/');
                var camelCaseResource = splitResourceString[splitResourceString.length - 1];
                if (camelCaseResource) {
                    //Convert the camel case to a friendly display name
                    camelCaseResource = camelCaseResource.trim();
                    var friendlyResourceDisplayName = '';
                    for (var i = 0; i < camelCaseResource.length; i++) {
                        if (/[A-Z]/.test(camelCaseResource[i]) &&
                        i !== 0 &&
                        /[a-z]/.test(camelCaseResource[i - 1])) {
                            friendlyResourceDisplayName += ' ';
                        }
                        if (i === 0 && /[a-z]/.test(camelCaseResource[i])) {
                            friendlyResourceDisplayName += camelCaseResource[i].toUpperCase();
                        } else {
                            friendlyResourceDisplayName += camelCaseResource[i];
                        }
                    }
                    return friendlyResourceDisplayName;
                }
            }
        }
    }
})();