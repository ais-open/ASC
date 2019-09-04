using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureServiceCatalog.Models
{
    public class ManagedIdentity
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("location")]
        public string Location { get; set; }
        [JsonProperty("clientId")]
        public string ClientId { get; set; }
        [JsonProperty("principalId")]
        public string PrincipalId { get; set; }
        [JsonProperty("tenantId")]
        public string TenantId { get; set; }
        [JsonProperty("tags")]
        public Object Tags { get; set; }
    }
}
