using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using AzureServiceCatalog.Models;

namespace AzureServiceCatalog.Helpers
{
    public class SecurityHelper
    {
        private RbacHelper rbacClient = new RbacHelper();

        public async Task<string> AddGroupsToAscContributorRole(string subscriptionId, string resourceGroup, List<ADGroup> contributorGroups, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "SecurityHelper:AddGroupsToAscContributorRole");
            try
            {
                dynamic role = await CreateAscContributorRoleIfNotExists(subscriptionId, thisOperationContext);
                foreach (var group in contributorGroups)
                {
                    await rbacClient.GrantRoleOnResourceGroup(subscriptionId, resourceGroup, (string)role.id, group.Id, thisOperationContext);
                }
                return role as string;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public async Task<bool> CheckUserPermissionToSubscription(string subscriptionId, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "SecurityHelper:CheckUserPermissionToSubscription");
            try
            {
                var repository = new TableCoreRepository();
                var subscription = repository.GetSubscription(subscriptionId, thisOperationContext);
                var currentUsersGroups = await Helpers.GetCurrentUserGroups(thisOperationContext);

                var contributorGroups = JArray.Parse(subscription.ContributorGroups);
                var ids = contributorGroups.Select(x => (string)x["id"]).ToList();
                return currentUsersGroups.Intersect(ids).Count() > 0;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        #region Private Members

        private async Task<dynamic> CreateAscContributorRoleIfNotExists(string subscriptionId, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "SecurityHelper:CreateAscContributorRoleIfNotExists");
            try
            {
                var role = await rbacClient.GetAscContributorRole(subscriptionId, thisOperationContext);
                if (role == null)
                {
                    var roleResult = await rbacClient.CreateAscContributorRoleOnSubscription(subscriptionId, thisOperationContext);
                    role = JObject.Parse(roleResult);

                    //even though the role is not found in the current subscription, if it already exists in another subscription of the same AzureAD tenant, the operation can error out if the same role is created again.
                    //Look for the specific error and just update the role definition, if RoleDefinition already exists in another subscription
                    if (role.error != null && role.error.code == "RoleDefinitionWithSameNameExists")
                    {
                        role = await UpdateAscContributorRole(subscriptionId, thisOperationContext);
                    }

                }
                return role;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        private async Task<dynamic> UpdateAscContributorRole(string subscriptionId, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "SecurityHelper:UpdateAscContributorRole");
            try
            {
                dynamic role = null;
                var coreRepository = new TableCoreRepository();
                var dbSubscriptions = coreRepository.GetSubscriptionListByOrgId(ClaimsPrincipal.Current.TenantId(), thisOperationContext);

                //Check in the subscriptions other than the current one for the Asc Contributor role
                List<string> subscriptionList = dbSubscriptions?.Where(s => s.Id != subscriptionId)?.Select(s => s.Id)?.ToList();

                if (subscriptionList != null && subscriptionList.Count > 0)
                {
                    var existingRole = await rbacClient.GetAscContributorRole(subscriptionList, thisOperationContext);
                    var roleResult = await rbacClient.UpdateAscContributorRoleOnSubscription(subscriptionId, existingRole, thisOperationContext);
                    role = JObject.Parse(roleResult);
                }

                return role;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        #endregion
    }
}