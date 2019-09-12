using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureServiceCatalog.Models
{
    public class BlueprintAssignment
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("blueprintName")]
        public string BlueprintName { get; set; }
        [JsonProperty("blueprintVersion")]
        public string BlueprintVersion { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("scope")]
        public string Scope { get; set; }
        [JsonProperty("location")]
        public string Location { get; set; }
        [JsonProperty("createdDate")]
        public string CreatedDate { get; set; }
        [JsonProperty("lastModifiedDate")]
        public string LastModifiedDate { get; set; }
        [JsonProperty("blueprintId")]
        public string BlueprintId { get; set; }
        [JsonProperty("provisioningState")]
        public string ProvisioningState { get; set; }
        [JsonProperty("lockMode")]
        public string LockMode { get; set; }
        [JsonProperty("managedIdentity")]
        public string ManagedIdentity { get; set; }
        [JsonProperty("resourceGroups")]
        public Object ResourceGroups { get; set; }
        [JsonProperty("parameters")]
        public Object Parameters { get; set; }
    }
}
