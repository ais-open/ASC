using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureServiceCatalog.Web.Models.Billing
{
    public class InstanceDataType
    {
        [JsonProperty("Microsoft.Resources")]
        public MicrosoftResourcesDataType MicrosoftResources { get; set; }
    }
}