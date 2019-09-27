﻿using Microsoft.Azure;
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
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using AzureServiceCatalog.Models;

namespace AzureServiceCatalog.Helpers
{
    public static class Helpers
    {
        //public const string AntaresApiVersion = "2014-06-01";
        public const string UserIdTagName = "UserId";
        public const string MSClientRequestHeader = "x-ms-client-request-id";

        public static ResourceManagementClient GetResourceManagementClient(string subscriptionId, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "Helpers:GetResourceManagementClient");
            try
            {
                ClientCredential credential = new ClientCredential(Config.ClientId, Config.Password);
                // initialize AuthenticationContext with the token cache of the currently signed in user, as kept in the app's EF DB
                var organizationId = ClaimsPrincipal.Current.TenantId();
                AuthenticationContext authContext = new AuthenticationContext(string.Format(Config.Authority, organizationId));
                AuthenticationResult result = authContext.AcquireToken(Config.AzureResourceManagerIdentifier, credential);

                var tokenCloudCredentials = new TokenCloudCredentials(subscriptionId: subscriptionId, token: result.AccessToken);
                var client = new ResourceManagementClient(tokenCloudCredentials, new Uri(Config.AzureResourceManagerUrl));
                client.HttpClient.DefaultRequestHeaders.Add(MSClientRequestHeader, Config.AscAppId);
                return client;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public static HttpClient GetAuthenticatedHttpClientForApp(BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "Helpers:GetAuthenticatedHttpClientForApp");
            try
            {
                return GetAuthenticatedHttpClient(AuthType.App, thisOperationContext);
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public static HttpClient GetAuthenticatedHttpClientForUser(BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "Helpers:GetAuthenticatedHttpClientForUser");
            try
            {
                return GetAuthenticatedHttpClient(AuthType.User, thisOperationContext);
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        private static HttpClient GetAuthenticatedHttpClient(AuthType authType, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "Helpers:GetAuthenticatedHttpClient");
            try
            {
                var authoritySegment = ClaimsPrincipal.Current.TenantId();
                var resourceIdentifier = Config.AzureResourceManagerIdentifier;

                string signedInUserUniqueName = ClaimsPrincipal.Current.FindFirst(ClaimTypes.Name).Value.Split('#').Last();
                ClientCredential credential = new ClientCredential(Config.ClientId, Config.Password);

                AuthenticationContext authContext = new AuthenticationContext(
                    string.Format(Config.Authority, authoritySegment), new AdalTokenCache(signedInUserUniqueName, thisOperationContext));

                AuthenticationResult result = (authType == AuthType.App ?
                    authContext.AcquireToken(resourceIdentifier, credential) :
                    authContext.AcquireTokenSilent(resourceIdentifier, credential,
                            new UserIdentifier(signedInUserUniqueName, UserIdentifierType.RequiredDisplayableId)));

                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
                httpClient.DefaultRequestHeaders.Add(HttpRequestHeader.Accept.ToString(), "application/json");
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
            return (principal.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid")!= null)?
                principal.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value : string.Empty;
        }

        public static List<string> Groups(this ClaimsPrincipal principal)
        {
            return principal.Claims.Where(x => x.Type == "groups").Select(x => x.Value).ToList();
        }

        public static string UserId(this ClaimsPrincipal principal)
        {
            return (principal.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier") != null) ?
                principal.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value : string.Empty;
        }

        public static string SignedInUserName(this ClaimsPrincipal principal)
        {
            return (principal.Claims.Where(c => c.Type.Contains("http://schemas.microsoft.com/identity/claims/objectidentifier"))!= null) ?
                principal.Claims.Where(c => c.Type.Contains("http://schemas.microsoft.com/identity/claims/objectidentifier")).First().Value : string.Empty;
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

        public static async Task<List<string>> GetCurrentUserGroups(BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "Helpers:GetCurrentUserGroups");
            try
            {
                List<string> userGroups = null;
                var userAdGroups = await AzureADGraphApiHelper.GetUserGroups(ClaimsPrincipal.Current.UserId(), ClaimsPrincipal.Current.TenantId(), thisOperationContext);
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
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
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