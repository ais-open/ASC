using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureServiceCatalog.Models
{
    public class BlueprintVersion
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("blueprintName")]
        public string BlueprintName { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("scope")]
        public string Scope { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("createdDate")]
        public string CreatedDate { get; set; }
        [JsonProperty("lastModifiedDate")]
        public string LastModifiedDate { get; set; }
        [JsonProperty("resourceGroups")]
        public Object ResourceGroups { get; set; }
        [JsonProperty("parameters")]
        public Object Parameters { get; set; }
    }

    public class BlueprintVersionResourceGroup
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("location")]
        public string Location { get; set; }
        [JsonProperty("isNameAvailable")]
        public bool IsNameAvailable { get; set; }
        [JsonProperty("isLocationAvailable")]
        public bool IsLocationAvailable { get; set; }
        [JsonProperty("dependsOn")]
        public JArray DependsOn { get; set; }
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
    }

    public class BlueprintVersionParameter
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("allowedValues")]
        public JArray AllowedValues { get; set; }
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
    }
}
