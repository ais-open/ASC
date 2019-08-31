using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureServiceCatalog.Models
{
    public class Blueprint
    {
        [JsonProperty("name")]
        public string Name { get; set; }
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
        [JsonProperty("propeties")]
        public Object Properties { get; set; }

    }
}
