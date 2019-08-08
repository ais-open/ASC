using Newtonsoft.Json;

namespace AzureServiceCatalog.Models
{
    public class ContributorGroup
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}