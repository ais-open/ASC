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

        public async Task<string> GetAssignedBlueprint(string subscriptionId, string assignmentName)
        {
            var requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/Microsoft.Blueprint/blueprintAssignments/{assignmentName}?api-version={blueprintApiVersion}";
            return await ArmHttpClient.Get(requestUrl);
        }

        public async Task<string> GetBlueprintDefinitions(string subscriptionId)
        {
            var requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/Microsoft.Blueprint/blueprints?api-version={blueprintApiVersion}";
            return await ArmHttpClient.Get(requestUrl);
        }

        public async Task<string> GetBlueprintVersions(string subscriptionId, string blueprintName)
        {
            var requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/Microsoft.Blueprint/blueprints/{blueprintName}/versions?api-version={blueprintApiVersion}";
            return await ArmHttpClient.Get(requestUrl);
        }

        public async Task<string> GetBlueprintVersion(string subscriptionId, string blueprintName, string versionName)
        {
            var requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/Microsoft.Blueprint/blueprints/{blueprintName}/versions/{versionName}?api-version={blueprintApiVersion}";
            return await ArmHttpClient.Get(requestUrl);
        }

        public async Task<string> AssignBlueprint(string subscriptionId, string assignmentName, object blueprintAssignment)
        {
            var requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/Microsoft.Blueprint/blueprintAssignments/{assignmentName}?api-version={blueprintApiVersion}";
            return await ArmHttpClient.Put(requestUrl, blueprintAssignment);
        }

        public async Task<string> GetObjectIdOfBlueprintServicePrincipal(string subscriptionId, string assignmentName)
        {
            var requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/Microsoft.Blueprint/blueprintAssignments/{assignmentName}/WhoIsBlueprint?api-version={blueprintApiVersion}";
            return await ArmHttpClient.Post(requestUrl, null);
        }
    }
}