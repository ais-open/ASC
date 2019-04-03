using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureServiceCatalog.Web.Models
{
    public class Organization : TableEntity
    {
        public Organization()
        {
            PartitionKey = "Azure Service Catalog";
        }
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
        public string VerifiedDomain { get; set; }
        public string ObjectIdOfCloudSenseServicePrincipal { get; set; }
        public string CreateProductGroup { get; set; }
        public string CreateProductGroupName { get; set; }
        public string DeployGroup { get; set; }
        public string AdminGroup { get; set; }
        public string AdminGroupName { get; set; }
        public DateTime? EnrolledDate { get; set; }
        public List<ADGroup> OrganizationADGroups { get; internal set; }
    }
}