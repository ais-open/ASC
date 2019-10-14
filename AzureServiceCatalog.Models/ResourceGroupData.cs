using AzureServiceCatalog.Models.Billing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureServiceCatalog.Models
{
    public class ResourceGroupData
    {
        public string ResourceGroupName { get; set; }
        public List<ResourceUsageDetails> ResourcesUsages { get; set; }
        public List<ChartData> ChartData { get; set; }
    }
}