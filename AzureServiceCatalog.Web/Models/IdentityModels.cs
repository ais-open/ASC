using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace AzureServiceCatalog.Web.Models
{
    public class IdentityModels
    {
        private TableCoreRepository coreRepository = new TableCoreRepository();
        public async Task<string> GetStorageName()
        {
            string signedInUserUniqueId = ClaimsPrincipal.Current.SignedInUserName();
            CacheUserDetails cud = MemoryCacher.GetValue(signedInUserUniqueId) as CacheUserDetails;
            if (cud == null)
            {
                cud = await GetCurrentUserData();
                MemoryCacher.Add(signedInUserUniqueId, cud, DateTime.Now.AddMinutes(20));
            }
            return cud.StorageName;
        }

        public async Task<string> GetStorageKey()
        {
            string signedInUserUniqueId = ClaimsPrincipal.Current.SignedInUserName();
            CacheUserDetails cud = MemoryCacher.GetValue(signedInUserUniqueId) as CacheUserDetails;
            if (cud == null)
            {
                cud = await GetCurrentUserData();
                MemoryCacher.Add(signedInUserUniqueId, cud, DateTime.Now.AddMinutes(20));
            }
            return cud.StorageKey;
        }

        private async Task<CacheUserDetails> GetCurrentUserData()
        {
            var cacheUserDetails = new CacheUserDetails();
            var organizationId = ClaimsPrincipal.Current.TenantId();
            var subscriptions = this.coreRepository.GetSubscriptionListByOrgId(organizationId);
            var subscription = subscriptions.Where(x => x.IsConnected == true).FirstOrDefault();

            if (subscription != null)
            {
                cacheUserDetails.SubscriptionId = subscription.Id;
                cacheUserDetails.OrganizationId = subscription.OrganizationId;
                cacheUserDetails.StorageName = subscription.StorageName;
                //cacheUserDetails.StorageKey = AzureResourceManagerUtil.GetStorageAccountKeysUsingResource(subscription.Id, subscription.OrganizationId, subscription.StorageName);
                cacheUserDetails.StorageKey = await AzureResourceManagerUtil.GetStorageAccountKeysArm(subscription.Id, subscription.StorageName);
            }

            return cacheUserDetails;
        }
    }
}