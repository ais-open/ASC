using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureServiceCatalog.Web.Models
{
    public class CacheUserDetails
    {
        public string SubscriptionId { get; set; }
        public string OrganizationId { get; set; }
        public string StorageName { get; set; }
        public string StorageKey { get; set; }
    }
}