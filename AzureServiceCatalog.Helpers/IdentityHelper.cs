using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using System;
using AzureServiceCatalog.Models;

namespace AzureServiceCatalog.Helpers
{
    public class IdentityHelper
    {
        private TableCoreRepository coreRepository = new TableCoreRepository();
        public async Task<string> GetStorageName(BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "IdentityHelper:GetStorageName");
            try
            {
                string signedInUserUniqueId = ClaimsPrincipal.Current.SignedInUserName();
                CacheUserDetails cud = MemoryCacher.GetValue(signedInUserUniqueId, thisOperationContext) as CacheUserDetails;
                if (cud == null)
                {
                    cud = await GetCurrentUserData(thisOperationContext);
                    MemoryCacher.Add(signedInUserUniqueId, cud, DateTime.Now.AddMinutes(20), thisOperationContext);
                }
                return cud.StorageName;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public async Task<string> GetStorageKey(BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "IdentityHelper:GetStorageName");
            try
            {
                string signedInUserUniqueId = ClaimsPrincipal.Current.SignedInUserName();
                CacheUserDetails cud = MemoryCacher.GetValue(signedInUserUniqueId, thisOperationContext) as CacheUserDetails;
                if (cud == null)
                {
                    cud = await GetCurrentUserData(thisOperationContext);
                    MemoryCacher.Add(signedInUserUniqueId, cud, DateTime.Now.AddMinutes(20), thisOperationContext);
                }
                return cud.StorageKey;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        private async Task<CacheUserDetails> GetCurrentUserData(BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "IdentityHelper:GetCurrentUserData");
            try
            {
                var cacheUserDetails = new CacheUserDetails();
                var organizationId = ClaimsPrincipal.Current.TenantId();
                var subscriptions = this.coreRepository.GetSubscriptionListByOrgId(organizationId, thisOperationContext);
                var subscription = subscriptions.Where(x => x.IsConnected == true).FirstOrDefault();

                if (subscription != null)
                {
                    cacheUserDetails.SubscriptionId = subscription.Id;
                    cacheUserDetails.OrganizationId = subscription.OrganizationId;
                    cacheUserDetails.StorageName = subscription.StorageName;
                    //cacheUserDetails.StorageKey = AzureResourceManagerHelper.GetStorageAccountKeysUsingResource(subscription.Id, subscription.OrganizationId, subscription.StorageName);
                    cacheUserDetails.StorageKey = await AzureResourceManagerHelper.GetStorageAccountKeysArm(subscription.Id, subscription.StorageName, thisOperationContext);
                }

                return cacheUserDetails;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }
    }
}