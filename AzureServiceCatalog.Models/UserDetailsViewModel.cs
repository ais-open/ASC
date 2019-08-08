using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureServiceCatalog.Models
{
    public class UserDetailsViewModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string UserId {get;set;}
        public bool IsAdminOfSubscription { get; set; }
        public string ServicePrincipalId { get; set; }
        public string OrganizationId { get; set; }
        public bool IsEnrolled { get; set; }
        public bool SubscriptionIsConnected { get; set; }
        public bool SubscriptionNeedsRepair { get; set; }
        public string SubscriptionId { get; set; }
        public string SubscriptionName { get; set; }
        public string OrganizationName { get; set; }
        public bool CanDeploy { get; set; }
        public bool CanCreate { get; set; }
        public bool CanAdmin { get; set; }
    }
}