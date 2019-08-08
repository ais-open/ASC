using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureServiceCatalog.Models
{
    public class ActivationInfo
    {
        public UserDetailsViewModel HostSubscription { get; set; }
        public List<UserDetailsViewModel> EnrolledSubscriptions { get; } = new List<UserDetailsViewModel>();
        public Organization Organization { get; set; }

        public string ResourceGroup { get; set; }
        public string Location { get; set; }
    }
}