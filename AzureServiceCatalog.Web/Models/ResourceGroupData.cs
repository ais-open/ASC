using AzureServiceCatalog.Web.Models.Billing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureServiceCatalog.Web.Models
{
    public class ResourceGroupData
    {
        public string ResourceGroupName { get; set; }
        public List<ResourceUsage> ResourcesUsages { get; internal set; }
        public List<ChartData> ChartData { get; internal set; }
    }
}