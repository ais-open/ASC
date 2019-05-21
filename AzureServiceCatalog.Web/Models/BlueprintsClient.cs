using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace AzureServiceCatalog.Web.Models
{
    public class BlueprintsClient
    {
        private const string blueprintApiVersion = "2018-11-01-preview";

        public async Task<string> GetAssignedBlueprints(string subscriptionId)
        {
            var requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/Microsoft.Blueprint/blueprintAssignments?api-version={blueprintApiVersion}";
            return await ArmHttpClient.Get(requestUrl);
        }

        public async Task<string> GetBlueprintDefinitions(string subscriptionId)
        {
            var requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/Microsoft.Blueprint/blueprints?api-version={blueprintApiVersion}";
            return await ArmHttpClient.Get(requestUrl);
        }
    }
}