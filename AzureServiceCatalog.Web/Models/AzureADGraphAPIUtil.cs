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

namespace AzureServiceCatalog.Web.Models
{
    public static class AzureADGraphApiUtil
    {
        public const string GlobalAdministratorRoleName = "Company Administrator";

        public static Organization GetOrganizationDetails(string organizationId)
        {
            var organizaton = new Organization { Id = organizationId };
            var requestUrl = new Uri($"{Config.GraphAPIIdentifier}{organizationId}/tenantDetails?api-version={Config.GraphAPIVersion}");
            var httpClient = GetAuthenticatedHttpClientForGraphApiForUser();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            HttpResponseMessage response = httpClient.SendAsync(request).Result;

            string responseContent = response.Content.ReadAsStringAsync().Result;
            var orgResult = (Json.Decode(responseContent)).value;
            if (orgResult != null && orgResult.Length > 0)
            {
                var org = orgResult[0];
                organizaton.DisplayName = org.displayName;
                organizaton.VerifiedDomain = (org.verifiedDomains as IEnumerable<dynamic>).FirstOrDefault(x => x["default"])?.name;
            }

            return organizaton;
        }

        public static List<ADGroup> GetAllGroupsForOrganization(string organizationId, string filter = null)
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
            var httpClient = GetAuthenticatedHttpClientForGraphApiForUser();
            //var httpClient = Utils.GetAuthenticatedHttpClientForUser();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            HttpResponseMessage response = httpClient.SendAsync(request).Result;

            string responseContent = response.Content.ReadAsStringAsync().Result;
            var groups = Json.Decode(responseContent).value as IEnumerable<dynamic>;
            return groups.Select(x => new ADGroup
            {
                Name = (string)x.displayName,
                Id = (string)x.objectId,
                Description = (string)x.description
            }).ToList();
        }

        public static ADGroup CheckIfADGroupExistsByOrgName(string organizationId, string adGroupName)
        {
            Uri requestUrl = null;
            requestUrl = new Uri($"{Config.GraphAPIIdentifier}{organizationId}/groups?$filter=displayName eq '{adGroupName}'&api-version=2013-04-05");
            
            //var httpClient = GetAuthenticatedHttpClientForGraphApiForUser();
            var httpClient = Utils.GetAuthenticatedHttpClientForUser();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            HttpResponseMessage response = httpClient.SendAsync(request).Result;

            string responseContent = response.Content.ReadAsStringAsync().Result;
            var groups = Json.Decode(responseContent).value as IEnumerable<dynamic>;
            return groups.Select(x => new ADGroup
            {
                Name = (string)x.displayName,
                Id = (string)x.objectId,
                Description = (string)x.description
            }).FirstOrDefault();
        }

        public static List<ADGroup> GetUserGroups(string objectId, string organizationId)
        {
            var requestUrl = new Uri($"{Config.GraphAPIIdentifier}{organizationId}/users/{objectId}/memberOf?api-version=2013-04-05");
            var httpClient = GetAuthenticatedHttpClientForGraphApiForUser();

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            HttpResponseMessage response = httpClient.SendAsync(request).Result;

            string responseContent = response.Content.ReadAsStringAsync().Result;
            var groups = Json.Decode(responseContent).value as IEnumerable<dynamic>;
            return groups.Select(x => new ADGroup
            {
                Name = (string)x.displayName,
                Id = (string)x.objectId,
                GroupType = (string)x.objectType
            }).ToList();
        }
        public static async Task CreateGroup(string organizationId, string adGroupName)
        {
            var requestUrl = new Uri($"{Config.GraphAPIIdentifier}{organizationId}/groups?api-version=2013-04-05");
            var httpClient = Utils.GetAuthenticatedHttpClientForUser();

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUrl);

            var requestBody = new { displayName = adGroupName, mailNickname = adGroupName, mailEnabled = false, securityEnabled = true };
            request.Content = JsonConvert.SerializeObject(requestBody).ToStringContent();
            var response = await httpClient.SendAsync(request);
        }
        public static async Task<bool> AddUserToGroup(string organizationId, string groupId, string userId)
        {
            var requestUrl = new Uri($"{Config.GraphAPIIdentifier}{organizationId}/groups/{groupId}/$links/members?api-version=2013-04-05");
            var httpClient = Utils.GetAuthenticatedHttpClientForUser();

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
            var requestBody = new { url = $"https://graph.windows.net/{organizationId}/directoryObjects/{userId}" };
            request.Content = JsonConvert.SerializeObject(requestBody).ToStringContent();
            var response = await httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public static string GetObjectIdOfServicePrincipalInOrganization(string organizationId, string applicationId)
        {
            string objectId = null;

            var requestUrl = new Uri($"{Config.GraphAPIIdentifier}{organizationId}/servicePrincipals?api-version={Config.GraphAPIVersion}&$filter=appId eq '{applicationId}'");

            var httpClient = GetAuthenticatedHttpClientForGraphApiForApp();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            HttpResponseMessage response = httpClient.SendAsync(request).Result;

            // Endpoint should return JSON with one or none serviePrincipal object
            if (response.IsSuccessStatusCode)
            {
                string responseContent = response.Content.ReadAsStringAsync().Result;
                var servicePrincipalResult = (Json.Decode(responseContent)).value;
                if (servicePrincipalResult != null && servicePrincipalResult.Length > 0)
                {
                    objectId = servicePrincipalResult[0].objectId;
                }
            }

            return objectId;
        }

        public static bool IsGlobalAdministrator(List<ADGroup> groupsRoles)
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

        private static HttpClient GetAuthenticatedHttpClientForGraphApiForApp()
        {
            return GetAuthenticatedHttpClientForGraphApi(AuthType.App);
        }

        private static HttpClient GetAuthenticatedHttpClientForGraphApiForUser()
        {
            return GetAuthenticatedHttpClientForGraphApi(AuthType.User);
        }

        // Eventually we might refactor and move this method over to Utils or ArmHttpClient.
        // For now, just leave here since it's dedicated to Graph API.
        private static HttpClient GetAuthenticatedHttpClientForGraphApi(AuthType authType)
        {
            var authoritySegment = ClaimsPrincipal.Current.TenantId();
            var resourceIdentifier = Config.GraphAPIIdentifier;

            string signedInUserUniqueName = ClaimsPrincipal.Current.FindFirst(ClaimTypes.Name).Value.Split('#').Last();
            ClientCredential credential = new ClientCredential(Config.ClientId, Config.Password);

            AuthenticationContext authContext = new AuthenticationContext(
                string.Format(Config.Authority, authoritySegment), new AdalTokenCache(signedInUserUniqueName));

            AuthenticationResult result = (authType == AuthType.App ?
                authContext.AcquireToken(resourceIdentifier, credential) :
                result = authContext.AcquireTokenSilent(resourceIdentifier, credential,
                        new UserIdentifier(signedInUserUniqueName, UserIdentifierType.RequiredDisplayableId)));

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
            httpClient.DefaultRequestHeaders.Add(HttpRequestHeader.ContentType.ToString(), "application/json");
            httpClient.DefaultRequestHeaders.Add(Utils.MSClientRequestHeader, Config.AscAppId);
            return httpClient;
        }
    }
}