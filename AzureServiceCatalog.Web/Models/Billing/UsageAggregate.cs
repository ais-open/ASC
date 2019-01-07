using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web;

namespace AzureServiceCatalog.Web.Models.Billing
{
    public class UsageAggregate
    {
        public UsageAggregate()
        {
            Properties = new Properties();
        }
        public string Id { get; set; }
        public string Name { get; set; }
        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "Probably shouldn't have named it this - but need to maintain serialization for now.")]
        public string Type { get; set; }
        public Properties Properties { get; set; }
    }
}