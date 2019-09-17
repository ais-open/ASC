(function () {
    'use strict';

    angular.module('ascApp').factory('ascApi', ascApi);

    ascApi.$inject = ['$http', '$q', '$window', 'appSpinner', 'appStorage', 'dialogsService', 'toastr', '$rootScope'];

    function ascApi($http, $q, $window, appSpinner, appStorage, dialogs, toastr, $rootScope) {
        var service = {
            createCustomRole: createCustomRole,
            createDeployment: createDeployment,
            createResourceGroup: createResourceGroup,
            deleteOrganization: deleteOrganization,
            deletePolicy: deletePolicy,
            deletePolicyAssignment: deletePolicyAssignment,
            deleteTemplate: deleteTemplate,
            getAllBaseTempates: getAllBaseTempates,
            getAuditLogs: getAuditLogs,
            getDeployment: getDeployment,
            getDeploymentList: getDeploymentList,
            getDeploymentStatus: getDeploymentStatus,
            getOrganization: getOrganization,
            getOrganizationByDomain: getOrganizationByDomain,
            getOrganizationGroups: getOrganizationGroups,
            getPolicies: getPolicies,
            getPolicy: getPolicy,
            getPolicyAssignments: getPolicyAssignments,
            getPolicyAssignment: getPolicyAssignment,
            getResourceGroupsBySubscription: getResourceGroupsBySubscription,
            getResourceGroupResources: getResourceGroupResources,
            getResourceGroupChartData: getResourceGroupChartData,
            getResourceGroupFullData: getResourceGroupFullData,
            getSubscriptions: getSubscriptions,
            getAllAppEnrolledSubscriptions: getAllAppEnrolledSubscriptions,
            getEnrolledSubscriptions: getEnrolledSubscriptions,
            getEnrolledSubscription: getEnrolledSubscription,
            getStorageProvider: getStorageProvider,
            getTemplate: getTemplate,
            getTemplates: getTemplates,
            getTenants: getTenants,
            getToken: getToken,
            getUserResourceGroupsBySubscription: getUserResourceGroupsBySubscription,
            getIdentity: getIdentity,
            getIdentityFull: getIdentityFull,
            getQuickStartTemplates: getQuickStartTemplates,
            getQuickStartTemplate: getQuickStartTemplate,
            //updateManagement: updateManagement,
            downloadTemplate: downloadTemplate,

            saveActivation: saveActivation,
            saveOrganization: saveOrganization,
            savePolicy: savePolicy,
            savePolicyAssignment: savePolicyAssignment,
            saveSubscriptions: saveSubscriptions,
            saveSubscriptionsWithSecurity: saveSubscriptionsWithSecurity,
            saveTemplate: saveTemplate,
            validateDeployment: validateDeployment,
            sendFeedback: sendFeedback,
            getDefaultFeedbackInfo: getDefaultFeedbackInfo,
            sendEnrollmentSupportRequest: sendEnrollmentSupportRequest,
            getBlueprints: getBlueprints,
            getBlueprintVersions: getBlueprintVersions,
            getBlueprintAssignment: getBlueprintAssignment,
            getBlueprintAssignments: getBlueprintAssignments,
            getUserAssignedIdentities: getUserAssignedIdentities,
            assignBlueprint: assignBlueprint,
            getUsers: getUsers,
            getPortalUrl: getPortalUrl,
            setselectedSubcription: setselectedSubcription,
            getselectedSubscription: getselectedSubscription
        };

        return service;

        function setselectedSubcription(id) {
            $window.localStorage.setItem('selected-subscription', id);
        }

        function getselectedSubscription() {
            return $window.localStorage.getItem('selected-subscription');
        }

        function createCustomRole(subscriptionId) {
            return httpPut('/api/identity/asc-contributor?subscriptionId=' + subscriptionId);
        }

        function createDeployment(deployment) {
            return httpPost('/api/deployments', deployment);
        }

        function createResourceGroup(resourceGroup) {
            return httpPost('/api/resourceGroups', resourceGroup);
        }

        function deleteOrganization() {
            return httpDelete('api/organization').then(function () {
                //The DELETE organization controller api will remove the subscription and organization records from the core table storage
                //as such, all information with regards to storage accounts will be lost,
                //meaning OAuth authorized calls to the Service Catalog's API will result in multiple
                //500 bad request errors due to the server trying to use null values for storage keys.
                //Consquently, after any call to deleteOrganization, redirect the user to the logout sequence
                window.location.href = 'Account/SignOut';
            });
        }

        function deletePolicy(subscriptionId, definitionName) {
            return httpDelete('/api/policies/' + definitionName + '?subscriptionId=' + subscriptionId);
        }

        function deletePolicyAssignment(subscriptionId, policyAssignmentName) {
            return httpDelete('/api/policy-assignments/' + policyAssignmentName + '?subscriptionId=' + subscriptionId);
        }

        function deleteTemplate(name) {
            return httpDelete('/api/templates?name=' + name);
        }

        function getAllBaseTempates(baseTemplateNames) {
            var promises = [];
            _.forEach(baseTemplateNames, function (name) {
                promises.push(httpGet('/json/' + name + '.json', true));
            });

            return $q.all(promises).then(function (results) {
                return results;
            });
        }

        function getAuditLogs(subscriptionId, correlationId) {
            return httpGet('/api/auditlogs?subscriptionId=' + subscriptionId +
                '&correlationId=' + correlationId);
        }

        function getDeployment(resourceGroupName, deploymentName, subscriptionId) {
            return httpGet('/api/deployments?resourceGroupName=' + resourceGroupName +
                '&deploymentName=' + deploymentName +
                '&subscriptionId=' + subscriptionId, true);
        }

        function getDeploymentStatus(resourceGroupName, deploymentName, subscriptionId) {
            return httpGet('/api/deployments/status?resourceGroupName=' + resourceGroupName +
                '&deploymentName=' + deploymentName +
                '&subscriptionId=' + subscriptionId, true);
        }

        function getDeploymentList(resourceGroupName, subscriptionId) {
            return httpGet('/api/deployments/list?resourceGroupName=' + resourceGroupName +
                '&subscriptionId=' + subscriptionId);
        }

        function getOrganization() {
            return httpGet('/api/organization');
        }

        function getOrganizationByDomain(domain) {
            return httpGet('/api/organization?domain=' + domain);
        }

        function getOrganizationGroups(filter) {
            if (filter) {
                return httpGet('/api/identity/organization-groups?filter=' + filter);
            }
            return httpGet('/api/identity/organization-groups');
        }

        function getPolicies(subscriptionId) {
            return httpGet('/api/policies?subscriptionId=' + subscriptionId);
        }

        function getPolicy(subscriptionId, definitionName) {
            return httpGet('/api/policies/' + definitionName + '?subscriptionId=' + subscriptionId);
        }

        function getPolicyAssignments(subscriptionId) {
            return httpGet('/api/policy-assignments?subscriptionId=' + subscriptionId);
        }

        function getPolicyAssignment(subscriptionId, policyAssignmentName) {
            return httpGet('/api/policy-assignments/' + policyAssignmentName + '?subscriptionId=' + subscriptionId);
        }

        function getResourceGroupsBySubscription(subscriptionId) {
            return httpGet('/api/subscriptions/' + subscriptionId + '/resourceGroups');
        }

        function getUserResourceGroupsBySubscription(subscriptionId) {
            return httpGet('/api/subscriptions/' + subscriptionId + '/user-resource-groups');
        }

        function getResourceGroupResources(resourceGroupName, subscriptionId) {
            return httpGet('/api/subscriptions/' + subscriptionId + '/resourceGroups/' + resourceGroupName);
        }

        function getResourceGroupChartData(resourceGroupName, subscriptionId) {
            return httpGet('/api/subscriptions/' + subscriptionId + '/chartData/' + resourceGroupName);
        }

        function getResourceGroupFullData(resourceGroupName, subscriptionId) {
            return httpGet('/api/subscriptions/' + subscriptionId + '/resourceGroups/' + resourceGroupName + '/full');
        }

        function getStorageProvider(subscriptionId) {
            return httpGet('/api/providers/storage?subscriptionId=' + subscriptionId);
        }

        function getSubscriptions() {
            return httpGet('/api/subscriptions');
        }

        function getAllAppEnrolledSubscriptions() {
            return httpGet('/api/subscriptions/all-app-enrolled');
        }

        function getEnrolledSubscriptions() {
            return httpGet('/api/subscriptions/enrolled');
        }

        function getEnrolledSubscription(subscriptionId) {
            return httpGet('/api/subscriptions/enrolled?subscriptionId=' + subscriptionId);
        }

        function getTemplate(id) {
            return httpGet('/api/templates/' + id);
        }

        function getTemplates() {
            return httpGet('/api/templates');
        }

        function getTenants() {
            return httpGet('/api/identity/tenants');
        }

        function getToken() {
            return httpGet('/api/identity/token');
        }

        function downloadTemplate(id) {
            return window.open('api/templates/' + id + '/download');
        }

        function getQuickStartTemplates() {
            return httpGet('/api/quickStartTemplates');
        }

        function getQuickStartTemplate(quickStartTemplateGitHubUrl) {
            //return httpGet('/api/quickStartTemplate/' + quickStartTemplateUniqueDirectory);
            return $http({
                url: quickStartTemplateGitHubUrl + '?callback=JSON_CALLBACK',
                type: 'GET',
                dataType: 'text/plain'
            }).then(function (response) {
                return response.data;
            }, function (error) {
                console.log('**Error making HTTP request', error);
            });
        }

        //function updateManagement(subscriptionTransactionViewModel) {
        //    return httpPost('/api/Identity', subscriptionTransactionViewModel);
        //}

        function saveActivation(activationInfo) {
            return httpPost('/api/activation', activationInfo);
        }

        function saveOrganization(organization) {
            return httpPost('/api/organization', organization);
        }

        function savePolicy(subscriptionId, definitionName, policy) {
            return httpPut('/api/policies/' + definitionName + '?subscriptionId=' + subscriptionId, policy);
        }

        function savePolicyAssignment(subscriptionId, policyAssignmentName, policyAssignment) {
            return httpPut('/api/policy-assignments/' + policyAssignmentName + '?subscriptionId=' + subscriptionId, policyAssignment);
        }

        function saveSubscriptions(subscriptions) {
            return httpPost('/api/subscriptions', subscriptions);
        }

        function saveSubscriptionsWithSecurity(subscriptions) {
            return httpPost('/api/subscriptions/with-security', subscriptions);
        }

        function saveTemplate(template) {
            return httpPost('/api/templates', template);
        }

        function getIdentity() {
            return httpGet('api/Identity', true);
        }

        function getIdentityFull() {
            return httpGet('api/Identity/full');
        }

        function validateDeployment(deployment) {
            return httpPost('/api/deployments/validate', deployment);
        }

        function sendFeedback(feedback) {
            return httpPost('/api/feedback', feedback);
        }

        function getDefaultFeedbackInfo(feedback) {
            return httpGet('/api/feedback');
        }

        function sendEnrollmentSupportRequest(vm) {
            return httpPost('/api/enrollmentsupport', vm);
        }

        function getBlueprints(subscriptionId) {
            return httpGet('/api/blueprints?subscriptionId=' + subscriptionId);
        }

        function getBlueprintVersions(subscriptionId, blueprintName) {
            return httpGet('/api/blueprint-versions/' + blueprintName + '?subscriptionId=' + subscriptionId);
        }

        function getBlueprintAssignment(subscriptionId, blueprintAssignmentName) {
            return httpGet('/api/blueprint-assignments/' + blueprintAssignmentName + '?subscriptionId=' + subscriptionId);
        }

        function getBlueprintAssignments(subscriptionId) {
            return httpGet('/api/blueprint-assignments?subscriptionId=' + subscriptionId);
        }

        function getUserAssignedIdentities(subscriptionId) {
            return httpGet('/api/managed-identities/userAssignedIdentities/?subscriptionId=' + subscriptionId);
        }

        function assignBlueprint(subscriptionId, blueprintAssignmentName, blueprintAssignment) {
            return httpPut('/api/blueprint-assignments/' + blueprintAssignmentName + '?subscriptionId=' + subscriptionId, blueprintAssignment);
        }

        function getUsers() {
            return httpGet('/api/identity/users');
        }

        function getPortalUrl(extensionForUrl, extensionType) {
            return httpGet('/api/identity/portal-url?extensionForUrl=' + extensionForUrl+ '&extensionType=' + extensionType);
        }

        //#region Private Methods

        function httpDelete(url, data) {
            return httpExecute(url, 'DELETE', data);
        }

        function httpExecute(requestUrl, method, data, suppressSpinner) {
            if (!suppressSpinner) {
                var spinnerMsg = (method === 'GET' ? 'Retrieving Data...' : 'Saving Changes...');
                appSpinner.showSpinner(spinnerMsg);
            }

            return $http({
                url: requestUrl,
                method: method,
                data: data,
                headers: {
                    'asc-selected-tenant': appStorage.getSelectedTenant()
                }
                //headers: requestConfig.headers
            }).then(function (response) {
                if (!suppressSpinner) {
                    appSpinner.hideSpinner();
                }
                //console.log('**response from EXECUTE', response);
                return response.data;
            }, function (error) {
                console.log('**Error making HTTP request', error);
                handleError(error, requestUrl);
                if (!suppressSpinner) {
                    appSpinner.hideSpinner();
                }
                return $q.reject();
            });
        }

        function httpGet(url, suppressSpinner) {
            return httpExecute(url, 'GET', null, suppressSpinner);
        }

        function httpPatch(url, data) {
            return httpExecute(url, 'PATCH', data);
        }

        function httpPost(url, data) {
            return httpExecute(url, 'POST', data);
        }

        function httpPut(url, data) {
            return httpExecute(url, 'PUT', data);
        }

        function saveItem(url, item) {
            if (item.id) {
                return httpPatch(url + '/' + item.id, item);
            } else {
                return httpPost(url, item);
            }
        }

        function handleError(error, requestUrl) {
            var message = "An internal server error occured while performing operation \"" + requestUrl + "\". Please retry your request."
            if (error.status === 500) {
                dialogs.errorDialog(message, error.data, 'Close');
            } else {
                message = error.data.message + "while performing operation \"" + requestUrl + "\".";
                dialogs.errorDialog(message, error.data, 'Close');
            }
        }

        //#endregion
    }
})();