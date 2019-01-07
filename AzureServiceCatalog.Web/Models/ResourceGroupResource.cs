using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureServiceCatalog.Web.Models
{
    public class ResourceGroupResource
    {
        public string ResourceGroupName { get; set; }
        public string Location { get; set; }
        public string SubscriptionId { get; set; }

        public List<ADGroup> ContributorGroups { get; } = new List<ADGroup>();
    }
}