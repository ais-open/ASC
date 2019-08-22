using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Threading.Tasks;
using AzureServiceCatalog.Models;

namespace AzureServiceCatalog.Helpers
{
    public class AdalTokenCache : TokenCache
    {
        string User;
        PerUserTokenCache Cache;
        private TableCoreRepository coreRepository = new TableCoreRepository();
        // constructor
        public AdalTokenCache(string user, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "AdalTokenCache:AdalTokenCache");
            try
            {
                // associate the cache to the current user of the web app
                User = user;

                this.AfterAccess = AfterAccessNotification;
                this.BeforeAccess = BeforeAccessNotification;
                this.BeforeWrite = BeforeWriteNotification;

                // look up the entry in the DB
                Cache = this.coreRepository.GetPerUserTokenCacheListById(User, thisOperationContext).FirstOrDefault();
                // place the entry in memory
                this.Deserialize((Cache == null) ? null : Cache.cacheBits);
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        // clean up the DB
        public override void Clear()
        {
            base.Clear();
            this.coreRepository.ClearAllPerUserTokenCache();
        }

        // Notification raised before ADAL accesses the cache.
        // This is your chance to update the in-memory copy from the DB, if the in-memory version is stale
        void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            var thisOperationContext = new BaseOperationContext("AdalTokenCache:BeforeAccessNotification");
            try
            {
                if (Cache == null)
                {
                    // first time access
                    Cache = this.coreRepository.GetPerUserTokenCacheListById(User, thisOperationContext).FirstOrDefault();
                }
                else
                {   // retrieve last write from the DB
                    var status = from e in this.coreRepository.GetPerUserTokenCacheListById(User, thisOperationContext)
                                 select new
                                 {
                                     LastWrite = e.LastWrite
                                 };
                    // if the in-memory copy is older than the persistent copy
                    if (status.Count() > 0 && status.First().LastWrite > Cache.LastWrite)
                    //// read from from storage, update in-memory copy
                    {
                        Cache = this.coreRepository.GetPerUserTokenCacheListById(User, thisOperationContext).FirstOrDefault();
                    }
                }
                this.Deserialize((Cache == null) ? null : Cache.cacheBits);
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        // Notification raised after ADAL accessed the cache.
        // If the HasStateChanged flag is set, ADAL changed the content of the cache
        void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            var thisOperationContext = new BaseOperationContext("AdalTokenCache:AfterAccessNotification");
            try
            {
                // if state changed
                if (this.HasStateChanged)
                {
                    // check for an existing entry
                    Cache = this.coreRepository.GetPerUserTokenCacheListById(User, thisOperationContext).FirstOrDefault();
                    if (Cache == null)
                    {
                        // if no existing entry for that user, create a new one
                        Cache = new PerUserTokenCache
                        {
                            webUserUniqueId = User,
                        };
                    }

                    // update the cache contents and the last write timestamp
                    Cache.cacheBits = this.Serialize();
                    Cache.LastWrite = DateTime.Now;

                    // update the DB with modification or new entry
                    this.coreRepository.SavePerUserTokenCaches(Cache, thisOperationContext);
                    this.HasStateChanged = false;
                }
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
            
        }
        void BeforeWriteNotification(TokenCacheNotificationArgs args)
        {
            // if you want to ensure that no concurrent write take place, use this notification to place a lock on the entry
        }
    }
}