using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureServiceCatalog.Models
{
    public class UserSubscriptionInfo
    {
        public UserSubscriptionInfo()
        {
            //this.AdminSubscriptions = new List<UserDetailsViewModel>();
            //this.OrganizationSubscriptions = new List<UserDetailsViewModel>();
            //this.OtherOrganizations = new List<Organization>();
            this.Subscriptions = new List<UserDetailsViewModel>();
            IsActivatedByAdmin = false;
            IsGlobalAdministrator = false;
        }

        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool CanCreate { get; set; }
        public bool CanDeploy { get; set; }
        public bool CanAdmin { get; set; }
        public bool IsGlobalAdministrator { get; set; }
        /// <summary>
        /// Has Azure AD been activated by admin
        /// </summary>
        public bool IsActivatedByAdmin { get; set; }
        public string DefaultAdGroup { get; set; }
        public string DefaultResourceGroup { get; set; }
        public Organization Organization { get; set; }

        public List<ADGroup> OrganizationADGroups { get; set; }

        public List<UserDetailsViewModel> Subscriptions { get; set; }
    }
}