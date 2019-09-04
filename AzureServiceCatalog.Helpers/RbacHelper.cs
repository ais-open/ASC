using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using AzureServiceCatalog.Models;

namespace AzureServiceCatalog.Helpers
{
    public class RbacHelper
    {
        private const string apiVersion = "2015-07-01";
        //private const string apiVersion = "2016-02-01";

        public async Task<string> CreateAscContributorRoleOnSubscription(string subscriptionId, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "RbacHelper:CreateAscContributorRoleOnSubscription");
            try
            {
                var newRoleId = Guid.NewGuid().ToString();
                var requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/Microsoft.Authorization/roleDefinitions/{newRoleId}?api-version={apiVersion}";

                // Modify the "assignableScopes" in the base template
                var ascContributorJson = File.ReadAllText(HttpContext.Current.Server.MapPath("~/json/asc-contributor.json"));
                dynamic role = JObject.Parse(ascContributorJson);
                role.properties.assignableScopes = new JArray() as dynamic;
                role.properties.assignableScopes.Add($"/subscriptions/{subscriptionId}");
                var requestBody = role.ToString();

                var response = await ArmHttpHelper.Put(requestUrl, requestBody, thisOperationContext);
                return response;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public async Task<string> UpdateAscContributorRoleOnSubscription(string subscriptionId, dynamic role, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "RbacHelper:UpdateAscContributorRoleOnSubscription");
            try
            {
                var roleId = role.name;
                role.properties.assignableScopes.Add($"/subscriptions/{subscriptionId}");

                var requestBody = role.ToString();
                var requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/Microsoft.Authorization/roleDefinitions/{roleId}?api-version={apiVersion}";

                var response = await ArmHttpHelper.Put(requestUrl, requestBody, thisOperationContext);
                return response;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public async Task<string> CreateAscContributorRoleOnResourceGroup(string subscriptionId, string resourceGroup, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "RbacHelper:CreateAscContributorRoleOnResourceGroup");
            try
            {
                var newRoleId = Guid.NewGuid().ToString();
                var requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.Authorization/roleDefinitions/{newRoleId}?api-version={apiVersion}";// 2016-02-01";

                // Modify the "assignableScopes" in the base template
                var ascContributorJson = File.ReadAllText(HttpContext.Current.Server.MapPath("~/json/asc-contributor.json"));
                dynamic role = JObject.Parse(ascContributorJson);
                role.properties.assignableScopes = new JArray() as dynamic;
                role.properties.assignableScopes.Add($"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}");
                var requestBody = role.ToString();

                var response = await ArmHttpHelper.Put(requestUrl, requestBody, thisOperationContext);
                return response;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public async Task<dynamic> GetAscContributorRole(string subscriptionId, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "RbacHelper:GetAscContributorRole");
            try
            {
                string json = await GetRoleByName(subscriptionId, "ASC Contributor", thisOperationContext);
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
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
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
        
        public async Task<dynamic> GetAscContributorRole(List<string> subscriptionList, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "RbacHelper:GetAscContributorRole");
            try
            {
                dynamic existingRole = null;
                foreach (var subscription in subscriptionList)
                {
                    existingRole = await GetAscContributorRole(subscription, thisOperationContext);
                    if (existingRole != null)
                    {
                        break;
                    }
                }
                return existingRole;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public async Task<dynamic> GetAscContributorRole(string subscriptionId, string resourceGroup, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "RbacHelper:GetAscContributorRole");
            try
            {
                string json = await GetRoleByName(subscriptionId, resourceGroup, "ASC Contributor", thisOperationContext);
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
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public async Task<string> GetRoleByName(string subscriptionId, string roleName, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "RbacHelper:GetRoleByName");
            try
            {
                var requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/Microsoft.Authorization/roleDefinitions?api-version={apiVersion}&$filter=roleName%20eq%20'{roleName}'";
                var response = await ArmHttpHelper.Get(requestUrl, thisOperationContext);
                return response;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public async Task<string> GetRoleByName(string subscriptionId, string resourceGroup, string roleName, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "RbacHelper:GetRoleByName");
            try
            {
                var requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.Authorization/roleDefinitions?api-version={apiVersion}&$filter=roleName%20eq%20'{roleName}'";
                var response = await ArmHttpHelper.Get(requestUrl, thisOperationContext);
                return response;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public async Task<string> DeleteCustomRoleOnSubscription(string subscriptionId, string roleId, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "RbacHelper:DeleteCustomRoleOnSubscription");
            try
            {
                var requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/Microsoft.Authorization/roleDefinitions/{roleId}?api-version={apiVersion}";
                var response = await ArmHttpHelper.Delete(requestUrl, thisOperationContext);
                return response;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public async Task<string> GrantRoleOnSubscription(string subscriptionId, string roleId, string objectId, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "RbacHelper:GrantRoleOnSubscription");
            try
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
                var json = await ArmHttpHelper.Put(requestUrl, requestJson, thisOperationContext);
                return json;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public async Task<string> GrantRoleOnResourceGroup(string subscriptionId, string resourceGroup, string roleId, string objectId, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "RbacHelper:GrantRoleOnResourceGroup");
            try
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
                var json = await ArmHttpHelper.Put(requestUrl, requestJson, thisOperationContext);
                return json;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public async Task<string> GetRoleAssignmentsForAscContributor(string subscriptionId, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "RbacHelper:GetRoleAssignmentsForAscContributor");
            try
            {
                var ascContributorRole = await this.GetAscContributorRole(subscriptionId, thisOperationContext);

                var requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/Microsoft.Authorization/roleAssignments?api-version={apiVersion}&filter=atScope()";
                var response = await ArmHttpHelper.Get(requestUrl, thisOperationContext);
                dynamic roleAssignments = JObject.Parse(response);
                var items = roleAssignments.value as IEnumerable<dynamic>;
                var ascRoleAssignments = items.Where(x => x.properties.roleDefinitionId == ascContributorRole.id).ToList();
       
                return ascRoleAssignments.ToJArray().ToString();
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public async Task<string> GrantRoleForBlueprintAssignment(string subscriptionId, string roleId, string objectId, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "RbacHelper:GrantRoleForBlueprintAssignment");
            try
            {
                var newRoleAssignmentId = Guid.NewGuid().ToString();
                var requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/Microsoft.Authorization/roleAssignments/{newRoleAssignmentId}?api-version={apiVersion}";
                var requestBody = new
                {
                    properties = new
                    {
                        roleDefinitionId = $"/subscriptions/{subscriptionId}/providers/Microsoft.Authorization/roleDefinitions/8e3af657-a8ff-443c-a75c-2fe8c4bcb635",
                        principalId = objectId,
                        scope = "/subscriptions/" + subscriptionId
                    }
                };
                var requestJson = JsonConvert.SerializeObject(requestBody);
                var json = await ArmHttpHelper.Put(requestUrl, requestJson, thisOperationContext);
                return json;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }
    }
}