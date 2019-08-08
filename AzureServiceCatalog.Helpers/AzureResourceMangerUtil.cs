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
    public static class AzureResourceManagerUtil
    {
        public static async Task<string> GetUserResourceGroups(string subscriptionId)
        {
            const string apiVersion = "2015-01-01";
            string requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/resourcegroups?api-version={apiVersion}";
            var httpClient = Utils.GetAuthenticatedHttpClientForUser();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            HttpResponseMessage response = await httpClient.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();
            return result;
        }

        public static async Task<string> GetStorageProvider(string subscriptionId)
        {
            string tenantId = ClaimsPrincipal.Current.TenantId();
            string requestUrl = string.Format("{0}/subscriptions/{1}/providers/Microsoft.Storage?api-version={2}", Config.AzureResourceManagerUrl, subscriptionId, Config.AzureResourceManagerAPIVersion);
            //var httpClient = Utils.GetAuthenticatedHttpClientForUser();
            var httpClient = Utils.GetAuthenticatedHttpClientForApp();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            HttpResponseMessage response = await httpClient.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();
            return result;
        }

        public static async Task<string> GetResourcesProvider(string subscriptionId)
        {
            string tenantId = ClaimsPrincipal.Current.TenantId();
            string requestUrl = string.Format("{0}/subscriptions/{1}/providers/Microsoft.Resources?api-version={2}", Config.AzureResourceManagerUrl, subscriptionId, Config.AzureResourceManagerAPIVersion);
            var httpClient = Utils.GetAuthenticatedHttpClientForApp();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            HttpResponseMessage response = await httpClient.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();
            return result;
        }

        public static async Task<string> CreateServiceCatalogMetadataStorageAccount(string subscriptionId, string resourceGroupName)
        {
            string storageAccountName = "svccat" + BlobHelpers.RandomString(18);
            var parameters = new { storageAccountName = new { value = storageAccountName }, storageAccountType = new { value = "Standard_LRS" } };
            await CreateStorageAccountARM(subscriptionId, resourceGroupName, parameters);
            return storageAccountName;
        }

        public static async Task<string> GetStorageAccountKeysArm(string subscriptionId, string storageAccountName)
        {
            var storageAccountUri = await GetStorageAccountUri(subscriptionId, storageAccountName);
            string tenantId = ClaimsPrincipal.Current.TenantId();
            string requestUrl = string.Format("{0}{1}/listKeys?api-version={2}",
                Config.AzureResourceManagerUrl, storageAccountUri, "2015-05-01-preview");
            //TODO: need to consider a global change for api-version ("2015-05-01-preview")
            var httpClient = Utils.GetAuthenticatedHttpClientForApp();

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
            HttpResponseMessage response = await httpClient.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();
            dynamic json = JObject.Parse(result);
            return (string)json.key1;
        }

        public static async Task<string> GetSubscriptionsForUser()
        {
            const string apiVersion = "2015-01-01";
            string tenantId = ClaimsPrincipal.Current.TenantId();
            string requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions?api-version={apiVersion}";
            //TODO: need to consider a global change for api-version ("2015-05-01-preview")
            var httpClient = Utils.GetAuthenticatedHttpClientForUser();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            HttpResponseMessage response = await httpClient.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();
            return result;
        }

        public static async Task<DeploymentGetResult> GetDeploymentResultWithPolling(AscDeployment deployment, int pollInterval = 3000)
        {
            var isRunning = true;
            DeploymentGetResult deploymentResult = null;
            while (isRunning)
            {
                deploymentResult = await DeploymentHelper.GetDeployment(deployment.ResourceGroupName, deployment.DeploymentName, deployment.SubscriptionId);
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




        /// <summary>
        /// Gets the user organizations.
        /// </summary>
        /// <returns></returns>
        public static async Task<List<Organization>> GetUserOrganizations()
        {
            List<Organization> organizations = new List<Organization>();
            string tenantId = ClaimsPrincipal.Current.TenantId();
            var requestUrl = new Uri($"{Config.AzureResourceManagerUrl}/tenants?api-version={Config.AzureResourceManagerAPIVersion}");
            var httpClient = Utils.GetAuthenticatedHttpClientForApp();
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
                    var spid = await AzureADGraphApiUtil.GetObjectIdOfServicePrincipalInOrganization(organization.tenantId, Config.ClientId);

                    //if (!string.IsNullOrEmpty(spid))
                    if (spid != string.Empty)
                    {
                        Organization orgDetails = await AzureADGraphApiUtil.GetOrganizationDetails(organization.tenantId);
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

        /// <summary>
        /// Gets the user subscriptions.
        /// </summary>
        /// <param name="organizationId">The organization identifier.</param>
        /// <returns></returns>
        public static async Task<List<Subscription>> GetUserSubscriptions(string organizationId)
        {
            List<Subscription> subscriptions = new List<Subscription>();
            string signedInUserUniqueName = ClaimsPrincipal.Current.SignedInUserName();
            var requestUrl = new Uri($"{Config.AzureResourceManagerUrl}/subscriptions?api-version={Config.AzureResourceManagerAPIVersion}");
            HttpClient client = Utils.GetAuthenticatedHttpClientForUser();
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

        /// <summary>
        /// Users the can manage access for subscription.
        /// </summary>
        /// <param name="subscriptionId">The subscription identifier.</param>
        /// <returns></returns>
        public static async Task<bool> UserCanManageAccessForSubscription(string subscriptionId)
        {
            bool ret = false;
            string signedInUserUniqueName = ClaimsPrincipal.Current.FindFirst(ClaimTypes.Name).Value.Split('#')[ClaimsPrincipal.Current.FindFirst(ClaimTypes.Name).Value.Split('#').Length - 1];
            var requestUrl = new Uri($"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/microsoft.authorization/permissions?api-version={Config.ARMAuthorizationPermissionsAPIVersion}");
            HttpClient client = Utils.GetAuthenticatedHttpClientForUser();
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

        /// <summary>
        /// Services the principal has read access to subscription.
        /// </summary>
        /// <param name="subscriptionId">The subscription identifier.</param>
        /// <returns></returns>
        public static async Task<bool> ServicePrincipalHasReadAccessToSubscription(string subscriptionId)
        {
            bool ret = false;
            var requestUrl = new Uri($"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/microsoft.authorization/permissions?api-version={Config.ARMAuthorizationPermissionsAPIVersion}");
            HttpClient client = Utils.GetAuthenticatedHttpClientForApp();
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

        /// <summary>
        /// Grants the role to service principal on subscription.
        /// </summary>
        /// <param name="objectId">The object identifier.</param>
        /// <param name="subscriptionId">The subscription identifier.</param>
        public static async Task GrantRoleToServicePrincipalOnSubscriptionAsync(string objectId, string subscriptionId)
        {
            string signedInUserUniqueName = ClaimsPrincipal.Current.SignedInUserName();
            string roleAssignmentId = Guid.NewGuid().ToString();
            string roleDefinitionId = await GetRoleId(ConfigurationManager.AppSettings["ida:RequiredARMRoleOnSubscription"], subscriptionId);
            var requestUrl = new Uri($"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/microsoft.authorization/roleassignments/{roleAssignmentId}?api-version={Config.ARMAuthorizationRoleAssignmentsAPIVersion}");

            HttpClient client = Utils.GetAuthenticatedHttpClientForUser();
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
            HttpResponseMessage response = client.SendAsync(request).Result;
            if (!response.IsSuccessStatusCode)
            {
                var result = response.Content.ReadAsStringAsync().Result;
                dynamic jsonResponse = JObject.Parse(result);
                var errorCode = (string)jsonResponse.error.code;
                if (errorCode != ErrorCodes.RoleAssignmentExists)
                {
                    throw new HttpUnhandledException(result);
                }
            }
        }

        /// <summary>
        /// Revokes the role from service principal on subscription.
        /// </summary>
        /// <param name="objectId">The service principal object id.</param>
        /// <param name="subscriptionId">The subscription identifier.</param>
        public static async Task RevokeRoleFromServicePrincipalOnSubscription(string objectId, string subscriptionId)
        {
            string signedInUserUniqueName = ClaimsPrincipal.Current.SignedInUserName();
            var requestUrl = new Uri($"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/microsoft.authorization/roleassignments?api-version={Config.ARMAuthorizationRoleAssignmentsAPIVersion}&$filter=principalId eq '{objectId}'");
            HttpClient client = Utils.GetAuthenticatedHttpClientForApp();
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

        /// <summary>
        /// Gets the role identifier.
        /// </summary>
        /// <param name="roleName">Name of the role.</param>
        /// <param name="subscriptionId">The subscription identifier.</param>
        /// <returns></returns>
        public static async Task<string> GetRoleId(string roleName, string subscriptionId)
        {
            string roleId = null;
            string signedInUserUniqueName = ClaimsPrincipal.Current.SignedInUserName();
            var requestUrl = new Uri($"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/Microsoft.Authorization/roleDefinitions?api-version={Config.ARMAuthorizationRoleDefinitionsAPIVersion}");
            HttpClient client = Utils.GetAuthenticatedHttpClientForUser();
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

        /// <summary>
        /// Adds the current user as contributor.
        /// </summary>
        /// <param name="subscriptionId">The subscription identifier.</param>
        /// <param name="resourceGroupName">Name of the resource group.</param>
        /// <returns></returns>
        public static async Task<string> AddCurrentUserAsContributor(string subscriptionId, string resourceGroupName)
        {
            string signedInUserUniqueId = ClaimsPrincipal.Current.SignedInUserName();
            var requestUrl = new Uri($"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/resourcegroups/{resourceGroupName}/providers/Microsoft.Authorization/roleAssignments/{Guid.NewGuid()}?api-version=2014-10-01-preview");
            HttpClient client = Utils.GetAuthenticatedHttpClientForApp();
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

        public static async Task<string> GetAuditLogs(string subscriptionId, string correlationId)
        {
            const string apiVersion = "2014-04-01";
            var timestamp = DateTime.Now.Subtract(TimeSpan.FromDays(30)).ToString("yyyy-MM-ddT00:00:00Z");
            var requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/microsoft.insights/eventtypes/management/values?api-version={apiVersion}&$filter=eventTimestamp ge '{timestamp}' and correlationId eq '{correlationId}'";
            var response = await ArmHttpClient.Get(requestUrl);
            return response;
        }

        #region Private Members

        private static async Task<DeploymentGetResult> CreateStorageAccountARM(string subscriptionId, string resourceGroupName, object parameters)
        {
            var deployment = new AscDeployment
            {
                Template = File.ReadAllText(HttpContext.Current.Server.MapPath("~/json/storage-account.json")),
                DeploymentName = "SVC-CAT-" + DateTime.Now.ToString("hh.mm.ss"),
                SubscriptionId = subscriptionId,
                ResourceGroupName = resourceGroupName,
                Parameters = JsonConvert.SerializeObject(parameters)
            };

            var result = await DeploymentHelper.Deploy(deployment);
            var deploymentResult = await GetDeploymentResultWithPolling(deployment);
            return deploymentResult;
        }

        private static async Task<string> GetStorageAccountUri(string subscriptionId, string storageAccountName)
        {
            string tenantId = ClaimsPrincipal.Current.TenantId();
            string requestUrl = string.Format("{0}/subscriptions/{1}/providers/Microsoft.Storage/storageAccounts?api-version={2}",
                Config.AzureResourceManagerUrl, subscriptionId, "2015-05-01-preview");
            var httpClient = Utils.GetAuthenticatedHttpClientForApp();

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

        private static class ErrorCodes
        {
            public const string RoleAssignmentExists = "RoleAssignmentExists";
        }
        #endregion
    }
}