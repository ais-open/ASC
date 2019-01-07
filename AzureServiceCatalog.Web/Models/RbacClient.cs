using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AzureServiceCatalog.Web.Models
{
    public class RbacClient
    {
        private const string apiVersion = "2015-07-01";
        //private const string apiVersion = "2016-02-01";

        public async Task<string> CreateAscContributorRoleOnSubscription(string subscriptionId)
        {
            var newRoleId = Guid.NewGuid().ToString();
            var requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/Microsoft.Authorization/roleDefinitions/{newRoleId}?api-version={apiVersion}";

            // Modify the "assignableScopes" in the base template
            var ascContributorJson = File.ReadAllText(HttpContext.Current.Server.MapPath("~/json/asc-contributor.json"));
            dynamic role = JObject.Parse(ascContributorJson);
            role.properties.assignableScopes = new JArray() as dynamic;
            role.properties.assignableScopes.Add($"/subscriptions/{subscriptionId}");
            var requestBody = role.ToString();

            var response = await ArmHttpClient.Put(requestUrl, requestBody);
            return response;
        }

        public async Task<string> UpdateAscContributorRoleOnSubscription(string subscriptionId, dynamic role)
        {
            var roleId = role.name;
            role.properties.assignableScopes.Add($"/subscriptions/{subscriptionId}");

            var requestBody = role.ToString();
            var requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/Microsoft.Authorization/roleDefinitions/{roleId}?api-version={apiVersion}";

            var response = await ArmHttpClient.Put(requestUrl, requestBody);
            return response;
        }

        public async Task<string> CreateAscContributorRoleOnResourceGroup(string subscriptionId, string resourceGroup)
        {
            var newRoleId = Guid.NewGuid().ToString();
            var requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.Authorization/roleDefinitions/{newRoleId}?api-version={apiVersion}";// 2016-02-01";

            // Modify the "assignableScopes" in the base template
            var ascContributorJson = File.ReadAllText(HttpContext.Current.Server.MapPath("~/json/asc-contributor.json"));
            dynamic role = JObject.Parse(ascContributorJson);
            role.properties.assignableScopes = new JArray() as dynamic;
            role.properties.assignableScopes.Add($"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}");
            var requestBody = role.ToString();

            var response = await ArmHttpClient.Put(requestUrl, requestBody);
            return response;
        }

        public async Task<dynamic> GetAscContributorRole(string subscriptionId)
        {
            string json = await GetRoleByName(subscriptionId, "ASC Contributor");
            dynamic result = JObject.Parse(json);
            var roles = result.value as IEnumerable<dynamic>;
            if (roles != null && roles.Count() == 0)
            {
                return null;
            }
            else
            {
                return roles.First();
            }
        }

        /// <summary>
        /// Look for the AscContributor role in any of the provided subscriptions
        /// </summary>
        /// <param name="subscriptionList"></param>
        /// <returns></returns>
        /// <remarks>
        /// For a Azure AD Tenant, a role can be defined only once. If the same role needs to be assigned for additional subscriptions, 
        /// then we need to look for the role in all the subscriptions within an AD to make sure it was not already created.
        /// </remarks>
        
        public async Task<dynamic> GetAscContributorRole(List<string> subscriptionList)
        {
            dynamic existingRole = null;
            foreach (var subscription in subscriptionList)
            {
                existingRole = await GetAscContributorRole(subscription);
                if (existingRole != null)
                {
                    break;
                }
            }
            return existingRole;
        }

        public async Task<dynamic> GetAscContributorRole(string subscriptionId, string resourceGroup)
        {
            string json = await GetRoleByName(subscriptionId, resourceGroup, "ASC Contributor");
            dynamic result = JObject.Parse(json);
            var roles = result.value as IEnumerable<dynamic>;
            if (roles != null && roles.Count() == 0)
            {
                return null;
            }
            else
            {
                return roles.First();
            }
        }

        public async Task<string> GetRoleByName(string subscriptionId, string roleName)
        {
            var requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/Microsoft.Authorization/roleDefinitions?api-version={apiVersion}&$filter=roleName%20eq%20'{roleName}'";
            var response = await ArmHttpClient.Get(requestUrl);
            return response;
        }

        public async Task<string> GetRoleByName(string subscriptionId, string resourceGroup, string roleName)
        {
            var requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.Authorization/roleDefinitions?api-version={apiVersion}&$filter=roleName%20eq%20'{roleName}'";
            var response = await ArmHttpClient.Get(requestUrl);
            return response;
        }

        public async Task<string> DeleteCustomRoleOnSubscription(string subscriptionId, string roleId)
        {
            var requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/Microsoft.Authorization/roleDefinitions/{roleId}?api-version={apiVersion}";
            var response = await ArmHttpClient.Delete(requestUrl);
            return response;
        }

        public async Task<string> GrantRoleOnSubscription(string subscriptionId, string roleId, string objectId)
        {
            var newRoleAssignmentId = Guid.NewGuid().ToString();
            var requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/microsoft.authorization/roleassignments/{newRoleAssignmentId}?api-version={Config.ARMAuthorizationRoleAssignmentsAPIVersion}";
            var requestBody = new
            {
                properties = new
                {
                    roleDefinitionId = roleId,
                    principalId = objectId
                }
            };
            var requestJson = JsonConvert.SerializeObject(requestBody);
            var json = await ArmHttpClient.Put(requestUrl, requestJson);
            return json;
        }

        public async Task<string> GrantRoleOnResourceGroup(string subscriptionId, string resourceGroup, string roleId, string objectId)
        {
            var newRoleAssignmentId = Guid.NewGuid().ToString();
            var requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/microsoft.authorization/roleassignments/{newRoleAssignmentId}?api-version={Config.ARMAuthorizationRoleAssignmentsAPIVersion}";
            var requestBody = new
            {
                properties = new
                {
                    roleDefinitionId = roleId,
                    principalId = objectId
                }
            };
            var requestJson = JsonConvert.SerializeObject(requestBody);
            var json = await ArmHttpClient.Put(requestUrl, requestJson);
            return json;
        }

        public async Task<string> GetRoleAssignmentsForAscContributor(string subscriptionId)
        {
            var ascContributorRole = await this.GetAscContributorRole(subscriptionId);

            var requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/Microsoft.Authorization/roleAssignments?api-version={apiVersion}&filter=atScope()";
            var response = await ArmHttpClient.Get(requestUrl);
            dynamic roleAssignments = JObject.Parse(response);
            var items = roleAssignments.value as IEnumerable<dynamic>;
            var ascRoleAssignments = items.Where(x => x.properties.roleDefinitionId == ascContributorRole.id).ToList();

            return ascRoleAssignments.ToJArray().ToString();
        }
    }
}