using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureServiceCatalog.Web.Models
{
    public class Subscription: TableEntity
    {
        public Subscription()
        {
            PartitionKey = "Azure Service Catalog";
            RowKey = Guid.NewGuid().ToString();
        }
        //public string Id { get; set; }
        public string Id
        {
            get
            {
                return this.RowKey;
            }
            set
            {
                this.RowKey = value;
            }
        }

        public string DisplayName { get; set; }
        public string OrganizationId { get; set; }
        public string StorageName { get; set; }
        public bool IsEnrolled { get; set; }
        [NotMapped]
        public bool IsConnected { get; set; }
        public DateTime ConnectedOn { get; set; }
        public string ConnectedBy { get; set; }
        [NotMapped]
        public bool AzureAccessNeedsToBeRepaired { get; set; }

        public string ContributorGroups { get; set; }
    }

}