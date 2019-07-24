using Microsoft.Azure;
using Microsoft.Azure.Management.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Configuration;
using System.Security.Claims;
using System.Net.Http.Headers;
using System.Net;
using Newtonsoft.Json.Linq;
using AzureServiceCatalog.Models;

namespace AzureServiceCatalog.Web.Models
{
    public static class Utils
    {
        //public const string AntaresApiVersion = "2014-06-01";
        public const string UserIdTagName = "UserId";
        public const string MSClientRequestHeader = "x-ms-client-request-id";

        public static ResourceManagementClient GetResourceManagementClient(string subscriptionId)
        {
            ClientCredential credential = new ClientCredential(Config.ClientId, Config.Password);
            // initialize AuthenticationContext with the token cache of the currently signed in user, as kept in the app's EF DB
            var organizationId = ClaimsPrincipal.Current.TenantId();
            AuthenticationContext authContext = new AuthenticationContext(string.Format(Config.Authority, organizationId));
            AuthenticationResult result = authContext.AcquireToken(Config.AzureResourceManagerIdentifier, credential);

            var tokenCloudCredentials = new TokenCloudCredentials(subscriptionId: subscriptionId, token: result.AccessToken);
            var client = new ResourceManagementClient(tokenCloudCredentials);
            client.HttpClient.DefaultRequestHeaders.Add(MSClientRequestHeader, Config.AscAppId);
            return client;
        }

        public static HttpClient GetAuthenticatedHttpClientForApp()
        {
            return GetAuthenticatedHttpClient(AuthType.App);
        }

        public static HttpClient GetAuthenticatedHttpClientForUser()
        {
            return GetAuthenticatedHttpClient(AuthType.User);
        }

        private static HttpClient GetAuthenticatedHttpClient(AuthType authType)
        {
            var authoritySegment = ClaimsPrincipal.Current.TenantId();
            var resourceIdentifier = Config.AzureResourceManagerIdentifier;

            string signedInUserUniqueName = ClaimsPrincipal.Current.FindFirst(ClaimTypes.Name).Value.Split('#').Last();
            ClientCredential credential = new ClientCredential(Config.ClientId, Config.Password);

            AuthenticationContext authContext = new AuthenticationContext(
                string.Format(Config.Authority, authoritySegment), new AdalTokenCache(signedInUserUniqueName));

            AuthenticationResult result = (authType == AuthType.App ?
                authContext.AcquireToken(resourceIdentifier, credential) :
                authContext.AcquireTokenSilent(resourceIdentifier, credential,
                        new UserIdentifier(signedInUserUniqueName, UserIdentifierType.RequiredDisplayableId)));

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
            httpClient.DefaultRequestHeaders.Add(HttpRequestHeader.Accept.ToString(), "application/json");
            httpClient.DefaultRequestHeaders.Add(HttpRequestHeader.ContentType.ToString(), "application/json");
            httpClient.DefaultRequestHeaders.Add(Utils.MSClientRequestHeader, Config.AscAppId);

            return httpClient;
        }

        public static string GetSignedInUserId()
        {
            return ClaimsPrincipal.Current.Claims.Where(c => c.Type.Contains("http://schemas.microsoft.com/identity/claims/objectidentifier")).First().Value;
        }

        public static KeyValuePair<string, string> GetUserIdTag()
        {
            return new KeyValuePair<string, string>(UserIdTagName, GetSignedInUserId());
        }

        public static long ParseInt64(string value)
        {
            long result;
            if (long.TryParse(value, out result))
                return result;
            return 0;
        }

        public static string TenantId(this ClaimsPrincipal principal)
        {
            return principal.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value;
        }

        public static List<string> Groups(this ClaimsPrincipal principal)
        {
            return principal.Claims.Where(x => x.Type == "groups").Select(x => x.Value).ToList();
        }

        public static string UserId(this ClaimsPrincipal principal)
        {
            return principal.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
        }

        public static string SignedInUserName(this ClaimsPrincipal principal)
        {
            return principal.Claims.Where(c => c.Type.Contains("http://schemas.microsoft.com/identity/claims/objectidentifier")).First().Value;
        }

        public static string FirstName(this ClaimsPrincipal principal)
        {
            return (principal.Claims.Where(c => c.Type == ClaimTypes.GivenName).Any()) ?
               principal.Claims.Where(c => c.Type == ClaimTypes.GivenName).First()?.Value : string.Empty;
        }

        public static string LastName(this ClaimsPrincipal principal)
        {
            return (principal.Claims.Where(c => c.Type == ClaimTypes.Surname).Any()) ?
              principal.Claims.Where(c => c.Type == ClaimTypes.Surname).First()?.Value : string.Empty;
        }

        public static string EmailAddress(this ClaimsPrincipal principal)
        {
            return (principal.Claims.Where(c => c.Type == ClaimTypes.Email).Any()) ?
              principal.Claims.Where(c => c.Type == ClaimTypes.Email).First()?.Value : string.Empty;
        }

        public static string Name(this ClaimsPrincipal principal)
        {
            return (principal.Claims.Where(c => c.Type == ClaimTypes.Name).Any()) ?
              principal.Claims.Where(c => c.Type == ClaimTypes.Name).First()?.Value : string.Empty;
        }

        public static string Upn(this ClaimsPrincipal principal)
        {
            return (principal.Claims.Where(c => c.Type == ClaimTypes.Upn).Any()) ?
              principal.Claims.Where(c => c.Type == ClaimTypes.Upn).First()?.Value : string.Empty;
        }

        public static List<string> GetCurrentUserGroups()
        {
            List<string> userGroups = null;
            var userAdGroups = AzureADGraphApiUtil.GetUserGroups(ClaimsPrincipal.Current.UserId(), ClaimsPrincipal.Current.TenantId());
            if (userAdGroups != null)
            {
                //During enrollment a new AD group is added. This group will not be available in Claims until user logs out and logs in again.
                //So we first try to get the user groups directly from the AD
                userGroups = userAdGroups.Select(x => x.Id).ToList();
            }
            else
            {
                userGroups = ClaimsPrincipal.Current.Groups();
            }

            return userGroups;
        }

        public static Double ParseDouble(string value)
        {
            double result;
            if (double.TryParse(value, out result))
                return result;
            return 0;

        }

        public static double ToJavaScriptTicks(this DateTime dt)
        {
            return dt.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }

        public static DateTime? ParseDateUtc(this string value)
        {
            DateTime result;
            if (DateTime.TryParse(value, out result))
            {
                return result.ToUniversalTime();
            }
            return null;
        }

        public static DateTime GetCurrentDateWithMidnightTime()
        {
            DateTime currentDateTime = DateTime.UtcNow;
            return new DateTime(currentDateTime.Year, currentDateTime.Month, currentDateTime.Day, 0, 0, 0, 0, DateTimeKind.Utc);
        }

        public static DateTime GetDateWithMidnightTime(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, value.Day, 0, 0, 0, 0, DateTimeKind.Utc);
        }

        public static StringContent ToStringContent(this string value)
        {
            return new StringContent(value, System.Text.Encoding.UTF8, "application/json");
        }

        public static bool IsMinDate(this DateTime value) => value == DateTime.MinValue;

        public static JArray ToJArray(this List<dynamic> list)
        {
            var results = new JArray();
            foreach (var item in list)
            {
                results.Add(item);
            }
            return results;
        }
    }
}