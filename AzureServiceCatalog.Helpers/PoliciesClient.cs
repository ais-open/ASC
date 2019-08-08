using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using AzureServiceCatalog.Models;

namespace AzureServiceCatalog.Helpers
{
    public class PoliciesClient
    {
        private const string policyApiVersion = "2015-10-01-preview";

        public async Task<string> GetPolicies(string subscriptionId)
        {
            var requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/Microsoft.authorization/policydefinitions?api-version={policyApiVersion}";
            return await ArmHttpClient.Get(requestUrl);
        }

        public async Task<string> GetPolicy(string subscriptionId, string definitionName)
        {
            var requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/Microsoft.authorization/policydefinitions/{definitionName}?api-version={policyApiVersion}";
            return await ArmHttpClient.Get(requestUrl);
        }

        public async Task<string> SavePolicy(string subscriptionId, string definitionName, object policy)
        {
            var requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/Microsoft.authorization/policydefinitions/{definitionName}?api-version={policyApiVersion}";
            return await ArmHttpClient.Put(requestUrl, policy);
        }

        public async Task<string> DeletePolicy(string subscriptionId, string definitionName)
        {
            var requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/Microsoft.authorization/policydefinitions/{definitionName}?api-version={policyApiVersion}";
            return await ArmHttpClient.Delete(requestUrl);
        }

        public async Task<string> GetPolicyAssignments(string subscriptionId)
        {
            var requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/Microsoft.authorization/policyassignments?api-version={policyApiVersion}";
            return await ArmHttpClient.Get(requestUrl);
        }

        public async Task<string> GetPolicyAssignment(string subscriptionId, string policyAssignmentName)
        {
            var requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/Microsoft.authorization/policyassignments/{policyAssignmentName}?api-version={policyApiVersion}";
            return await ArmHttpClient.Get(requestUrl);
        }

        public async Task<string> SavePolicyAssignment(string subscriptionId, string policyAssignmentName, object policy)
        {
            var requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/Microsoft.authorization/policyassignments/{policyAssignmentName}?api-version={policyApiVersion}";
            return await ArmHttpClient.Put(requestUrl, policy);
        }

        public async Task<string> DeletePolicyAssignment(string subscriptionId, string policyAssignmentName)
        {
            var requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/Microsoft.authorization/policyassignments/{policyAssignmentName}?api-version={policyApiVersion}";
            return await ArmHttpClient.Delete(requestUrl);
        }
    }
}