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
using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;
using AzureServiceCatalog.Models;

namespace AzureServiceCatalog.Helpers
{
    public static class AzureADGraphApiHelper
    {
        public const string GlobalAdministratorRoleName = "Company Administrator";

        public static async Task<Organization> GetOrganizationDetails(string organizationId, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "AzureADGraphApiHelper: GetOrganizationDetails");
            try
            {
                var organizaton = new Organization { Id = organizationId };
                var requestUrl = new Uri($"{Config.GraphAPIIdentifier}{organizationId}/tenantDetails?api-version={Config.GraphAPIVersion}");
                var httpClient = GetAuthenticatedHttpClientForGraphApiForUser(thisOperationContext);

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                HttpResponseMessage response = await httpClient.SendAsync(request);

                string responseContent = await response.Content.ReadAsStringAsync();
                var orgResult = (Json.Decode(responseContent)).value;
                if (orgResult != null && orgResult.Length > 0)
                {
                    var org = orgResult[0];
                    organizaton.DisplayName = org.displayName;
                    organizaton.VerifiedDomain = (org.verifiedDomains as IEnumerable<dynamic>).FirstOrDefault(x => x["default"])?.name;
                }

                return organizaton;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public static async Task<List<ADGroup>> GetAllGroupsForOrganization(string organizationId, BaseOperationContext parentOperationContext, string filter = null)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "AzureADGraphApiHelper:GetAllGroupsForOrganization");
            try
            {
                Uri requestUrl = null;
                if (string.IsNullOrEmpty(filter))
                {
                    requestUrl = new Uri($"{Config.GraphAPIIdentifier}{organizationId}/groups?api-version=2013-04-05");
                }
                else
                {
                    requestUrl = new Uri($"{Config.GraphAPIIdentifier}{organizationId}/groups?$filter=startswith(displayName,'{filter}')&api-version=2013-04-05");
                }
                //var httpClient = GetAuthenticatedHttpClientForGraphApiForUser();
                var httpClient = Helpers.GetAuthenticatedHttpClientForUser(thisOperationContext);
                var isRunningInAzureGeneral = Config.IsRunningInAzureGeneral(Config.StorageAccountEndpointSuffix);
                if (!isRunningInAzureGeneral)
                {
                    httpClient = GetAuthenticatedHttpClientForGraphApiForUser(thisOperationContext);
                }
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                HttpResponseMessage response = await httpClient.SendAsync(request);

                string responseContent = await response.Content.ReadAsStringAsync();
                var groups = Json.Decode(responseContent).value as IEnumerable<dynamic>;
                return groups.Select(x => new ADGroup
                {
                    Name = (string)x.displayName,
                    Id = (string)x.objectId,
                    Description = (string)x.description
                }).ToList();
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public static async Task<ADGroup> CheckIfADGroupExistsByOrgName(string organizationId, string adGroupName, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "AzureADGraphApiHelper:CheckIfADGroupExistsByOrgName");
            try
            {
                Uri requestUrl = null;
                requestUrl = new Uri($"{Config.GraphAPIIdentifier}{organizationId}/groups?$filter=displayName eq '{adGroupName}'&api-version=2013-04-05");

                //var httpClient = GetAuthenticatedHttpClientForGraphApiForUser();
                var httpClient = Helpers.GetAuthenticatedHttpClientForUser(thisOperationContext);
                var isRunningInAzureGeneral = Config.IsRunningInAzureGeneral(Config.StorageAccountEndpointSuffix);
                if (!isRunningInAzureGeneral)
                {
                    httpClient = GetAuthenticatedHttpClientForGraphApiForUser(thisOperationContext);
                }
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                HttpResponseMessage response = await httpClient.SendAsync(request);

                string responseContent = await response.Content.ReadAsStringAsync();
                var groups = Json.Decode(responseContent).value as IEnumerable<dynamic>;
                return groups.Select(x => new ADGroup
                {
                    Name = (string)x.displayName,
                    Id = (string)x.objectId,
                    Description = (string)x.description
                }).FirstOrDefault();
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public static async Task<List<ADGroup>> GetUserGroups(string objectId, string organizationId, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "AzureADGraphApiHelper:GetUserGroups");
            try
            {
                var requestUrl = new Uri($"{Config.GraphAPIIdentifier}{organizationId}/users/{objectId}/memberOf?api-version=2013-04-05");
                var httpClient = Helpers.GetAuthenticatedHttpClientForUser(thisOperationContext);
                var isRunningInAzureGeneral = Config.IsRunningInAzureGeneral(Config.StorageAccountEndpointSuffix);
                if (!isRunningInAzureGeneral)
                {
                    httpClient = GetAuthenticatedHttpClientForGraphApiForUser(thisOperationContext);
                }
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                HttpResponseMessage response = await httpClient.SendAsync(request);

                string responseContent = await response.Content.ReadAsStringAsync();
                var groups = Json.Decode(responseContent).value as IEnumerable<dynamic>;
                return groups.Select(x => new ADGroup
                {
                    Name = (string)x.displayName,
                    Id = (string)x.objectId,
                    GroupType = (string)x.objectType
                }).ToList();
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public static async Task CreateGroup(string organizationId, string adGroupName, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "AzureADGraphApiHelper:CreateGroup");
            try
            {
                var requestUrl = new Uri($"{Config.GraphAPIIdentifier}{organizationId}/groups?api-version=2013-04-05");
                var httpClient = Helpers.GetAuthenticatedHttpClientForUser(thisOperationContext);
                var isRunningInAzureGeneral = Config.IsRunningInAzureGeneral(Config.StorageAccountEndpointSuffix);
                if (!isRunningInAzureGeneral)
                {
                    httpClient = GetAuthenticatedHttpClientForGraphApiForUser(thisOperationContext);
                }
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUrl);

                var requestBody = new { displayName = adGroupName, mailNickname = adGroupName, mailEnabled = false, securityEnabled = true };
                request.Content = JsonConvert.SerializeObject(requestBody).ToStringContent();
                var response = await httpClient.SendAsync(request);
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public static async Task<bool> AddUserToGroup(string organizationId, string groupId, string userId, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "AzureADGraphApiHelper:AddUserToGroup");
            try
            {
                var requestUrl = new Uri($"{Config.GraphAPIIdentifier}{organizationId}/groups/{groupId}/$links/members?api-version=2013-04-05");
                var httpClient = Helpers.GetAuthenticatedHttpClientForUser(thisOperationContext);
                var isRunningInAzureGeneral = Config.IsRunningInAzureGeneral(Config.StorageAccountEndpointSuffix);
                if (!isRunningInAzureGeneral)
                {
                    httpClient = GetAuthenticatedHttpClientForGraphApiForUser(thisOperationContext);
                }
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                var requestBody = new { url = $"https://graph.windows.net/{organizationId}/directoryObjects/{userId}" };
                request.Content = JsonConvert.SerializeObject(requestBody).ToStringContent();
                var response = await httpClient.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public static async Task<string> GetObjectIdOfServicePrincipalInOrganization(string organizationId, string applicationId, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "AzureADGraphApiHelper:GetObjectIdOfServicePrincipalInOrganization");
            try
            {
                string objectId = null;
                var requestUrl = new Uri($"{Config.GraphAPIIdentifier}{organizationId}/servicePrincipals?api-version={Config.GraphAPIVersion}&$filter=appId eq '{applicationId}'");
                var httpClient = GetAuthenticatedHttpClientForGraphApiForApp(thisOperationContext);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                HttpResponseMessage response = await httpClient.SendAsync(request);

                // Endpoint should return JSON with one or none serviePrincipal object
                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    var servicePrincipalResult = (Json.Decode(responseContent)).value;
                    if (servicePrincipalResult != null && servicePrincipalResult.Length > 0)
                    {
                        objectId = servicePrincipalResult[0].objectId;
                    }
                }

                return objectId;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public static bool IsGlobalAdministrator(List<ADGroup> groupsRoles,BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "AzureADGraphApiHelper:IsGlobalAdministrator");
            try
            {
                bool isAdmin = false;
                if (groupsRoles != null && groupsRoles.Count > 0)
                {
                    var adminRole = groupsRoles.Where(x => x.GroupType.Equals("Role", StringComparison.OrdinalIgnoreCase) && x.Name.Equals(GlobalAdministratorRoleName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    if (adminRole != null)
                    {
                        isAdmin = true;
                    }
                }

                return isAdmin;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            } 
        }

        private static HttpClient GetAuthenticatedHttpClientForGraphApiForApp(BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "AzureADGraphApiHelper:GetAuthenticatedHttpClientForGraphApiForApp");
            try
            {
                return GetAuthenticatedHttpClientForGraphApi(AuthType.App, thisOperationContext);
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        private static HttpClient GetAuthenticatedHttpClientForGraphApiForUser(BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "AzureADGraphApiHelper:GetAuthenticatedHttpClientForGraphApiForUser");
            try
            {
                return GetAuthenticatedHttpClientForGraphApi(AuthType.User, thisOperationContext);
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        // Eventually we might refactor and move this method over to Utils or ArmHttpClient.
        // For now, just leave here since it's dedicated to Graph API.
        private static HttpClient GetAuthenticatedHttpClientForGraphApi(AuthType authType, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "AzureADGraphApiHelper:GetAuthenticatedHttpClientForGraphApi");
            try
            {
                var authoritySegment = ClaimsPrincipal.Current.TenantId();
                var resourceIdentifier = Config.GraphAPIIdentifier;

                string signedInUserUniqueName = ClaimsPrincipal.Current.FindFirst(ClaimTypes.Name).Value.Split('#').Last();
                ClientCredential credential = new ClientCredential(Config.ClientId, Config.Password);

                AuthenticationContext authContext = new AuthenticationContext(
                    string.Format(Config.Authority, authoritySegment), new AdalTokenCache(signedInUserUniqueName, thisOperationContext));

                AuthenticationResult result = (authType == AuthType.App ?
                    authContext.AcquireToken(resourceIdentifier, credential) :
                    result = authContext.AcquireTokenSilent(resourceIdentifier, credential,
                            new UserIdentifier(signedInUserUniqueName, UserIdentifierType.RequiredDisplayableId)));

                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
                httpClient.DefaultRequestHeaders.Add(HttpRequestHeader.ContentType.ToString(), "application/json");
                httpClient.DefaultRequestHeaders.Add(Helpers.MSClientRequestHeader, Config.AscAppId);
                return httpClient;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }
    }
}