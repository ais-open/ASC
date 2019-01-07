using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace AzureServiceCatalog.Web.Models
{
    public class SecurityProcessor
    {
        private RbacClient rbacClient = new RbacClient();

        public async Task<string> AddGroupsToAscContributorRole(string subscriptionId, string resourceGroup, List<ADGroup> contributorGroups)
        {
            dynamic role = await CreateAscContributorRoleIfNotExists(subscriptionId);
            foreach (var group in contributorGroups)
            {
                await rbacClient.GrantRoleOnResourceGroup(subscriptionId, resourceGroup, (string)role.id, group.Id);
            }
            return role as string;
        }

        public bool CheckUserPermissionToSubscription(string subscriptionId)
        {
            var repository = new TableCoreRepository();
            var subscription = repository.GetSubscription(subscriptionId);
            var currentUsersGroups = Utils.GetCurrentUserGroups();

            var contributorGroups = JArray.Parse(subscription.ContributorGroups);
            var ids = contributorGroups.Select(x => (string)x["id"]).ToList();
            return currentUsersGroups.Intersect(ids).Count() > 0;
        }

        #region Private Members

        private async Task<dynamic> CreateAscContributorRoleIfNotExists(string subscriptionId)
        {
            var role = await rbacClient.GetAscContributorRole(subscriptionId);
            if (role == null)
            {
                var roleResult = await rbacClient.CreateAscContributorRoleOnSubscription(subscriptionId);
                role = JObject.Parse(roleResult);

                //even though the role is not found in the current subscription, if it already exists in another subscription of the same AzureAD tenant, the operation can error out if the same role is created again.
                //Look for the specific error and just update the role definition, if RoleDefinition already exists in another subscription
                if (role.error != null && role.error.code == "RoleDefinitionWithSameNameExists")
                {
                    role = await UpdateAscContributorRole(subscriptionId);
                }

            }
            return role;
        }

        private async Task<dynamic> UpdateAscContributorRole(string subscriptionId)
        {
            dynamic role = null;
            var coreRepository = new TableCoreRepository();
            var dbSubscriptions = coreRepository.GetSubscriptionListByOrgId(ClaimsPrincipal.Current.TenantId());

            //Check in the subscriptions other than the current one for the Asc Contributor role
            List<string> subscriptionList = dbSubscriptions?.Where(s => s.Id != subscriptionId)?.Select(s => s.Id)?.ToList();

            if (subscriptionList != null && subscriptionList.Count > 0)
            {
                var existingRole = await rbacClient.GetAscContributorRole(subscriptionList);
                var roleResult = await rbacClient.UpdateAscContributorRoleOnSubscription(subscriptionId, existingRole);
                role = JObject.Parse(roleResult);
            }

            return role;
        }

        #endregion
    }
}