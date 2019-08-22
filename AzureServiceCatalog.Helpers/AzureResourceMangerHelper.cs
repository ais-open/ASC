using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Security.Cryptography;
using System.Text;
using System.Globalization;
using Newtonsoft.Json.Linq;
using System.Xml.Linq;
using System.Xml;
using System.IO;
using System.Threading;
using System.Dynamic;
using Newtonsoft.Json;
using System.Diagnostics;
using Microsoft.Azure.Management.Resources.Models;
using AzureServiceCatalog.Models;

namespace AzureServiceCatalog.Helpers
{
    /// <summary>
    /// helper utility methods
    /// </summary>
    public static class AzureResourceManagerHelper
    {
        public static async Task<string> GetUserResourceGroups(string subscriptionId, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "AzureResourceManagerHelper: GetUserResourceGroups");
            try
            {
                const string apiVersion = "2015-01-01";
                string requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/resourcegroups?api-version={apiVersion}";
                var httpClient = Helpers.GetAuthenticatedHttpClientForUser(thisOperationContext);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                HttpResponseMessage response = await httpClient.SendAsync(request);
                var result = await response.Content.ReadAsStringAsync();
                return result;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public static async Task<string> GetStorageProvider(string subscriptionId, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "AzureResourceManagerHelper: GetStorageProvider");
            try
            {
                string tenantId = ClaimsPrincipal.Current.TenantId();
                string requestUrl = string.Format("{0}/subscriptions/{1}/providers/Microsoft.Storage?api-version={2}", Config.AzureResourceManagerUrl, subscriptionId, Config.AzureResourceManagerAPIVersion);
                //var httpClient = Utils.GetAuthenticatedHttpClientForUser();
                var httpClient = Helpers.GetAuthenticatedHttpClientForApp(thisOperationContext);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                HttpResponseMessage response = await httpClient.SendAsync(request);
                var result = await response.Content.ReadAsStringAsync();
                return result;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public static async Task<string> GetResourcesProvider(string subscriptionId, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "AzureResourceManagerHelper: GetResourcesProvider");
            try
            {
                string tenantId = ClaimsPrincipal.Current.TenantId();
                string requestUrl = string.Format("{0}/subscriptions/{1}/providers/Microsoft.Resources?api-version={2}", Config.AzureResourceManagerUrl, subscriptionId, Config.AzureResourceManagerAPIVersion);
                var httpClient = Helpers.GetAuthenticatedHttpClientForApp(thisOperationContext);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                HttpResponseMessage response = await httpClient.SendAsync(request);
                var result = await response.Content.ReadAsStringAsync();
                return result;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public static async Task<string> CreateServiceCatalogMetadataStorageAccount(string subscriptionId, string resourceGroupName, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "AzureResourceManagerHelper: CreateServiceCatalogMetadataStorageAccount");
            try
            {
                string storageAccountName = "svccat" + BlobHelpers.RandomString(18);
                var parameters = new { storageAccountName = new { value = storageAccountName }, storageAccountType = new { value = "Standard_LRS" } };
                await CreateStorageAccountARM(subscriptionId, resourceGroupName, parameters, thisOperationContext);
                return storageAccountName;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public static async Task<string> GetStorageAccountKeysArm(string subscriptionId, string storageAccountName, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "AzureResourceManagerHelper: GetStorageAccountKeysArm");
            try
            {
                var storageAccountUri = await GetStorageAccountUri(subscriptionId, storageAccountName, thisOperationContext);
                string tenantId = ClaimsPrincipal.Current.TenantId();
                string requestUrl = string.Format("{0}{1}/listKeys?api-version={2}",
                    Config.AzureResourceManagerUrl, storageAccountUri, "2015-05-01-preview");
                //TODO: need to consider a global change for api-version ("2015-05-01-preview")
                var httpClient = Helpers.GetAuthenticatedHttpClientForApp(thisOperationContext);

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                HttpResponseMessage response = await httpClient.SendAsync(request);
                var result = await response.Content.ReadAsStringAsync();
                dynamic json = JObject.Parse(result);
                return (string)json.key1;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public static async Task<string> GetSubscriptionsForUser(BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "AzureResourceManagerHelper: GetSubscriptionsForUser");
            try
            {
                const string apiVersion = "2015-01-01";
                string tenantId = ClaimsPrincipal.Current.TenantId();
                string requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions?api-version={apiVersion}";
                //TODO: need to consider a global change for api-version ("2015-05-01-preview")
                var httpClient = Helpers.GetAuthenticatedHttpClientForUser(thisOperationContext);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                HttpResponseMessage response = await httpClient.SendAsync(request);
                var result = await response.Content.ReadAsStringAsync();
                return result;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public static async Task<DeploymentGetResult> GetDeploymentResultWithPolling(AscDeployment deployment, BaseOperationContext parentOperationContext, int pollInterval = 3000)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "AzureResourceManagerHelper: GetDeploymentResultWithPolling");
            try
            {
                var isRunning = true;
                DeploymentGetResult deploymentResult = null;
                while (isRunning)
                {
                    deploymentResult = await DeploymentHelper.GetDeployment(deployment.ResourceGroupName, deployment.DeploymentName, deployment.SubscriptionId, thisOperationContext);
                    switch (deploymentResult.Deployment.Properties.ProvisioningState)
                    {
                        case "Accepted":
                        case "Running":
                            Thread.Sleep(pollInterval);
                            break;

                        case "Failed":
                        case "Succeeded":
                            isRunning = false;
                            break;
                    }
                }
                if (deploymentResult.Deployment.Properties.ProvisioningState == "Failed")
                {
                    throw new ApplicationException("Error deploying Azure Resource. " +
                        "RequestId: " + deploymentResult.RequestId + ", " +
                        "Id: " + deploymentResult.Deployment.Id + ", " +
                        "CorrelationId: " + deploymentResult.Deployment.Properties.CorrelationId);
                }
                return deploymentResult;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        /// <summary>
        /// Gets the user organizations.
        /// </summary>
        /// <returns></returns>
        public static async Task<List<Organization>> GetUserOrganizations(BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "AzureResourceManagerHelper: GetUserOrganizations");
            try
            {
                List<Organization> organizations = new List<Organization>();
                string tenantId = ClaimsPrincipal.Current.TenantId();
                var requestUrl = new Uri($"{Config.AzureResourceManagerUrl}/tenants?api-version={Config.AzureResourceManagerAPIVersion}");
                var httpClient = Helpers.GetAuthenticatedHttpClientForApp(thisOperationContext);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                HttpResponseMessage response = await httpClient.SendAsync(request);

                // Endpoint returns JSON with an array of Tenant Objects
                // id                                            tenantId
                // --                                            --------
                // /tenants/7fe877e6-a150-4992-bbfe-f517e304dfa0 7fe877e6-a150-4992-bbfe-f517e304dfa0
                // /tenants/62e173e9-301e-423e-bcd4-29121ec1aa24 62e173e9-301e-423e-bcd4-29121ec1aa24

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    var organizationsResult = (Json.Decode(responseContent)).value;
                    foreach (var organization in organizationsResult)
                    {
                        var spid = await AzureADGraphApiHelper.GetObjectIdOfServicePrincipalInOrganization(organization.tenantId, Config.ClientId, thisOperationContext);

                        //if (!string.IsNullOrEmpty(spid))
                        if (spid != string.Empty)
                        {
                            Organization orgDetails = await AzureADGraphApiHelper.GetOrganizationDetails(organization.tenantId, thisOperationContext);
                            organizations.Add(new Organization()
                            {
                                Id = organization.tenantId,
                                DisplayName = orgDetails.DisplayName,
                                VerifiedDomain = orgDetails.VerifiedDomain,
                                ObjectIdOfCloudSenseServicePrincipal = spid
                            });
                        }
                    }
                }
                return organizations;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        /// <summary>
        /// Gets the user subscriptions.
        /// </summary>
        /// <param name="organizationId">The organization identifier.</param>
        /// <returns></returns>
        public static async Task<List<Subscription>> GetUserSubscriptions(string organizationId, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "AzureResourceManagerHelper: GetUserSubscriptions");
            try
            {
                List<Subscription> subscriptions = new List<Subscription>();
                string signedInUserUniqueName = ClaimsPrincipal.Current.SignedInUserName();
                var requestUrl = new Uri($"{Config.AzureResourceManagerUrl}/subscriptions?api-version={Config.AzureResourceManagerAPIVersion}");
                HttpClient client = Helpers.GetAuthenticatedHttpClientForUser(thisOperationContext);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                HttpResponseMessage response = await client.SendAsync(request);

                // Endpoint returns JSON with an array of Subscription Objects
                // id                                                  subscriptionId                       displayName state
                // --                                                  --------------                       ----------- -----
                // /subscriptions/c276fc76-9cd4-44c9-99a7-4fd71546436e c276fc76-9cd4-44c9-99a7-4fd71546436e Production  Enabled
                // /subscriptions/e91d47c4-76f3-4271-a796-21b4ecfe3624 e91d47c4-76f3-4271-a796-21b4ecfe3624 Development Enabled

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    var subscriptionsResult = (Json.Decode(responseContent)).value;

                    foreach (var subscription in subscriptionsResult)
                    {
                        if (subscription.state != "Disabled")
                        {
                            subscriptions.Add(new Subscription()
                            {
                                Id = subscription.subscriptionId,
                                DisplayName = subscription.displayName,
                                OrganizationId = organizationId
                            });
                        }
                    }
                }
                return subscriptions;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
            
        }

        /// <summary>
        /// Users the can manage access for subscription.
        /// </summary>
        /// <param name="subscriptionId">The subscription identifier.</param>
        /// <returns></returns>
        public static async Task<bool> UserCanManageAccessForSubscription(string subscriptionId, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "AzureResourceManagerHelper: UserCanManageAccessForSubscription");
            try
            {
                bool ret = false;
                string signedInUserUniqueName = ClaimsPrincipal.Current.FindFirst(ClaimTypes.Name).Value.Split('#')[ClaimsPrincipal.Current.FindFirst(ClaimTypes.Name).Value.Split('#').Length - 1];
                var requestUrl = new Uri($"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/microsoft.authorization/permissions?api-version={Config.ARMAuthorizationPermissionsAPIVersion}");
                HttpClient client = Helpers.GetAuthenticatedHttpClientForUser(thisOperationContext);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                HttpResponseMessage response = await client.SendAsync(request);

                // Endpoint returns JSON with an array of Actions and NotActions
                // actions  notActions
                // -------  ----------
                // {*}      {Microsoft.Authorization/*/Write, Microsoft.Authorization/*/Delete}
                // {*/read} {}

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    var permissionsResult = (Json.Decode(responseContent)).value;

                    foreach (var permissions in permissionsResult)
                    {
                        bool permissionMatch = false;
                        foreach (string action in permissions.actions)
                        {
                            var actionPattern = "^" + Regex.Escape(action.ToLower()).Replace("\\*", ".*") + "$";
                            permissionMatch = Regex.IsMatch("microsoft.authorization/roleassignments/write", actionPattern);
                            if (permissionMatch) break;
                        }
                        // if one of the actions match, check that the NotActions don't
                        if (permissionMatch)
                        {
                            foreach (string notAction in permissions.notActions)
                            {
                                var notActionPattern = "^" + Regex.Escape(notAction.ToLower()).Replace("\\*", ".*") + "$";
                                if (Regex.IsMatch("microsoft.authorization/roleassignments/write", notActionPattern))
                                    permissionMatch = false;
                                if (!permissionMatch) break;
                            }
                        }
                        if (permissionMatch)
                        {
                            ret = true;
                            break;
                        }
                    }
                }

                return ret;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        /// <summary>
        /// Services the principal has read access to subscription.
        /// </summary>
        /// <param name="subscriptionId">The subscription identifier.</param>
        /// <returns></returns>
        public static async Task<bool> ServicePrincipalHasReadAccessToSubscription(string subscriptionId, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "AzureResourceManagerHelper: ServicePrincipalHasReadAccessToSubscription");
            try
            {
                bool ret = false;
                var requestUrl = new Uri($"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/microsoft.authorization/permissions?api-version={Config.ARMAuthorizationPermissionsAPIVersion}");
                HttpClient client = Helpers.GetAuthenticatedHttpClientForApp(thisOperationContext);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                HttpResponseMessage response = await client.SendAsync(request);

                // Endpoint returns JSON with an array of Actions and NotActions
                // actions  notActions
                // -------  ----------
                // {*}      {Microsoft.Authorization/*/Write, Microsoft.Authorization/*/Delete}
                // {*/read} {}

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    var permissionsResult = (Json.Decode(responseContent)).value;

                    foreach (var permissions in permissionsResult)
                    {
                        bool permissionMatch = false;
                        foreach (string action in permissions.actions)
                            if (action.Equals("*/read", StringComparison.CurrentCultureIgnoreCase) || action.Equals("*", StringComparison.CurrentCultureIgnoreCase))
                            {
                                permissionMatch = true;
                                break;
                            }
                        // if one of the actions match, check that the NotActions don't
                        if (permissionMatch)
                        {
                            foreach (string notAction in permissions.notActions)
                                if (notAction.Equals("*", StringComparison.CurrentCultureIgnoreCase) || notAction.EndsWith("/read", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    permissionMatch = false;
                                    break;
                                }
                        }
                        if (permissionMatch)
                        {
                            ret = true;
                            break;
                        }
                    }
                }

                return ret;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        /// <summary>
        /// Grants the role to service principal on subscription.
        /// </summary>
        /// <param name="objectId">The object identifier.</param>
        /// <param name="subscriptionId">The subscription identifier.</param>
        public static async Task GrantRoleToServicePrincipalOnSubscriptionAsync(string objectId, string subscriptionId, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "AzureResourceManagerHelper: GrantRoleToServicePrincipalOnSubscriptionAsync");
            try
            {
                string signedInUserUniqueName = ClaimsPrincipal.Current.SignedInUserName();
                string roleAssignmentId = Guid.NewGuid().ToString();
                string roleDefinitionId = await GetRoleId(ConfigurationManager.AppSettings["ida:RequiredARMRoleOnSubscription"], subscriptionId, thisOperationContext);
                var requestUrl = new Uri($"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/microsoft.authorization/roleassignments/{roleAssignmentId}?api-version={Config.ARMAuthorizationRoleAssignmentsAPIVersion}");

                HttpClient client = Helpers.GetAuthenticatedHttpClientForUser(thisOperationContext);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, requestUrl);
                var requestBody = new
                {
                    properties = new
                    {
                        roleDefinitionId = roleDefinitionId,
                        principalId = objectId
                    }
                };
                var json = JsonConvert.SerializeObject(requestBody);
                StringContent content = new StringContent(json);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                request.Content = content;
                HttpResponseMessage response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    dynamic jsonResponse = JObject.Parse(result);
                    var errorCode = (string)jsonResponse.error.code;
                    if (errorCode != ErrorCodes.RoleAssignmentExists)
                    {
                        throw new HttpUnhandledException(result);
                    }
                }
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        /// <summary>
        /// Revokes the role from service principal on subscription.
        /// </summary>
        /// <param name="objectId">The service principal object id.</param>
        /// <param name="subscriptionId">The subscription identifier.</param>
        public static async Task RevokeRoleFromServicePrincipalOnSubscription(string objectId, string subscriptionId, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "AzureResourceManagerHelper: RevokeRoleFromServicePrincipalOnSubscription");
            try
            {
                string signedInUserUniqueName = ClaimsPrincipal.Current.SignedInUserName();
                var requestUrl = new Uri($"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/microsoft.authorization/roleassignments?api-version={Config.ARMAuthorizationRoleAssignmentsAPIVersion}&$filter=principalId eq '{objectId}'");
                HttpClient client = Helpers.GetAuthenticatedHttpClientForApp(thisOperationContext);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                HttpResponseMessage response = await client.SendAsync(request);

                // Endpoint returns JSON with an array of role assignments
                // properties                                  id                                          type                                        name
                // ----------                                  --                                          ----                                        ----
                // @{roleDefinitionId=/subscriptions/e91d47... /subscriptions/e91d47c4-76f3-4271-a796-2... Microsoft.Authorization/roleAssignments     9db2cdc1-2971-42fe-bd21-c7c4ead4b1b8

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    var roleAssignmentsResult = (Json.Decode(responseContent)).value;

                    //remove all role assignments
                    foreach (var roleAssignment in roleAssignmentsResult)
                    {
                        requestUrl = new Uri($"{Config.AzureResourceManagerUrl}{roleAssignment.id}?api-version={Config.ARMAuthorizationRoleAssignmentsAPIVersion}");
                        request = new HttpRequestMessage(HttpMethod.Delete, requestUrl);
                        response = client.SendAsync(request).Result;
                    }
                }
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        /// <summary>
        /// Gets the role identifier.
        /// </summary>
        /// <param name="roleName">Name of the role.</param>
        /// <param name="subscriptionId">The subscription identifier.</param>
        /// <returns></returns>
        public static async Task<string> GetRoleId(string roleName, string subscriptionId, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "AzureResourceManagerHelper: GetRoleId");
            try
            {
                string roleId = null;
                string signedInUserUniqueName = ClaimsPrincipal.Current.SignedInUserName();
                var requestUrl = new Uri($"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/Microsoft.Authorization/roleDefinitions?api-version={Config.ARMAuthorizationRoleDefinitionsAPIVersion}");
                HttpClient client = Helpers.GetAuthenticatedHttpClientForUser(thisOperationContext);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                HttpResponseMessage response = await client.SendAsync(request);

                // Endpoint returns JSON with an array of roleDefinition Objects
                // properties                                  id                                          type                                        name
                // ----------                                  --                                          ----                                        ----
                // @{roleName=Contributor; type=BuiltInRole... /subscriptions/e91d47c4-76f3-4271-a796-2... Microsoft.Authorization/roleDefinitions     b24988ac-6180-42a0-ab88-20f7382dd24c
                // @{roleName=Owner; type=BuiltInRole; desc... /subscriptions/e91d47c4-76f3-4271-a796-2... Microsoft.Authorization/roleDefinitions     8e3af657-a8ff-443c-a75c-2fe8c4bcb635
                // @{roleName=Reader; type=BuiltInRole; des... /subscriptions/e91d47c4-76f3-4271-a796-2... Microsoft.Authorization/roleDefinitions     acdd72a7-3385-48ef-bd42-f606fba81ae7
                // ...

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    var roleDefinitionsResult = (Json.Decode(responseContent)).value;

                    foreach (var roleDefinition in roleDefinitionsResult)
                        if ((roleDefinition.properties.roleName as string).Equals(roleName, StringComparison.CurrentCultureIgnoreCase))
                        {
                            roleId = roleDefinition.id;
                            break;
                        }
                }
                return roleId;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        /// <summary>
        /// Adds the current user as contributor.
        /// </summary>
        /// <param name="subscriptionId">The subscription identifier.</param>
        /// <param name="resourceGroupName">Name of the resource group.</param>
        /// <returns></returns>
        public static async Task<string> AddCurrentUserAsContributor(string subscriptionId, string resourceGroupName, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "AzureResourceManagerHelper: AddCurrentUserAsContributor");
            try
            {
                string signedInUserUniqueId = ClaimsPrincipal.Current.SignedInUserName();
                var requestUrl = new Uri($"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/resourcegroups/{resourceGroupName}/providers/Microsoft.Authorization/roleAssignments/{Guid.NewGuid()}?api-version=2014-10-01-preview");
                HttpClient client = Helpers.GetAuthenticatedHttpClientForApp(thisOperationContext);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, requestUrl);
                var requestBody = new
                {
                    properties = new
                    {
                        roleDefinitionId = $"/subscriptions/{subscriptionId}/providers/Microsoft.Authorization/roleDefinitions/b24988ac-6180-42a0-ab88-20f7382dd24c",
                        principalId = signedInUserUniqueId,
                        scope = $"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}"
                    }
                };
                var json = JsonConvert.SerializeObject(requestBody);
                var httpContent = new StringContent(json);
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                request.Content = httpContent;
                HttpResponseMessage response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }

                return "Failed";
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public static async Task<string> GetAuditLogs(string subscriptionId, string correlationId, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "AzureResourceMangerUtil:GetAuditLogs");
            try
            {
                const string apiVersion = "2014-04-01";
                var timestamp = DateTime.Now.Subtract(TimeSpan.FromDays(30)).ToString("yyyy-MM-ddT00:00:00Z");
                var requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/microsoft.insights/eventtypes/management/values?api-version={apiVersion}&$filter=eventTimestamp ge '{timestamp}' and correlationId eq '{correlationId}'";
                var response = await ArmHttpHelper.Get(requestUrl, thisOperationContext);
                return response;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        #region Private Members

        private static async Task<DeploymentGetResult> CreateStorageAccountARM(string subscriptionId, string resourceGroupName, object parameters, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "AzureResourceManagerHelper: CreateStorageAccountARM");
            try
            {
                var deployment = new AscDeployment
                {
                    Template = File.ReadAllText(HttpContext.Current.Server.MapPath("~/json/storage-account.json")),
                    DeploymentName = "SVC-CAT-" + DateTime.Now.ToString("hh.mm.ss"),
                    SubscriptionId = subscriptionId,
                    ResourceGroupName = resourceGroupName,
                    Parameters = JsonConvert.SerializeObject(parameters)
                };

                var result = await DeploymentHelper.Deploy(deployment, thisOperationContext);
                var deploymentResult = await GetDeploymentResultWithPolling(deployment, thisOperationContext);
                return deploymentResult;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }   
        }

        private static async Task<string> GetStorageAccountUri(string subscriptionId, string storageAccountName, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "AzureResourceManagerHelper: GetStorageAccountUri");
            try
            {
                string tenantId = ClaimsPrincipal.Current.TenantId();
                string requestUrl = string.Format("{0}/subscriptions/{1}/providers/Microsoft.Storage/storageAccounts?api-version={2}",
                    Config.AzureResourceManagerUrl, subscriptionId, "2015-05-01-preview");
                var httpClient = Helpers.GetAuthenticatedHttpClientForApp(thisOperationContext);

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                HttpResponseMessage response = await httpClient.SendAsync(request);
                /* {StatusCode: 403, ReasonPhrase: 'Forbidden', Version: 1.1, Content: System.Net.Http.StreamContent, Headers:
                    {
                        Pragma: no - cache
                      x - ms - failure - cause: gateway
                      x - ms - request - id: 2a7b5da1 - 8963 - 4e0c - 9e7c - c277d4c784b3
                      x - ms - correlation - request - id: 2a7b5da1 - 8963 - 4e0c - 9e7c - c277d4c784b3
                      x - ms - routing - request - id: EASTUS: 20170504T200623Z: 2a7b5da1 - 8963 - 4e0c - 9e7c - c277d4c784b3
                      Strict - Transport - Security: max - age = 31536000; includeSubDomains
                  Connection: close
                      Cache - Control: no - cache
                      Date: Thu, 04 May 2017 20:06:22 GMT
                      Content - Length: 309
                      Content - Type: application / json; charset = utf - 8
                      Expires: -1
                    }
                }*/
                var result = await response.Content.ReadAsStringAsync();
                dynamic json = JObject.Parse(result);
                var acct = (json.value as IEnumerable<dynamic>).Where(a => a.name == storageAccountName).SingleOrDefault();
                return (string)acct.id;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        private static class ErrorCodes
        {
            public const string RoleAssignmentExists = "RoleAssignmentExists";
        }
        #endregion
    }
}