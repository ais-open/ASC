using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureServiceCatalog.Models.Billing
{
    public class MicrosoftResourcesDataType
    {
        public string ResourceUri { get; set; }

        public IDictionary<string, string> Tags { get; }

        public IDictionary<string, string> AdditionalInfo { get; }

        public string Location { get; set; }

        public string PartNumber { get; set; }

        public string OrderNumber { get; set; }
    }
}