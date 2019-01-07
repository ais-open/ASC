using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureServiceCatalog.Web.Models
{
    public class SubscriptionsViewModel
    {
        public List<Subscription> Subscriptions { get; } = new List<Subscription>();

        public string ResourceGroup { get; set; }
        public string Location { get; set; }
    }
}