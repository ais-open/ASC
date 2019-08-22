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
    public class PoliciesHelper
    {
        private const string policyApiVersion = "2015-10-01-preview";

        public async Task<string> GetPolicies(string subscriptionId, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "PoliciesHelper:GetPolicies");
            try
            {
                var requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/Microsoft.authorization/policydefinitions?api-version={policyApiVersion}";
                return await ArmHttpHelper.Get(requestUrl, thisOperationContext);
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public async Task<string> GetPolicy(string subscriptionId, string definitionName, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "PoliciesHelper:GetPolicy");
            try
            {
                var requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/Microsoft.authorization/policydefinitions/{definitionName}?api-version={policyApiVersion}";
                return await ArmHttpHelper.Get(requestUrl, thisOperationContext);
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public async Task<string> SavePolicy(string subscriptionId, string definitionName, object policy, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "PoliciesHelper:SavePolicy");
            try
            {
                var requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/Microsoft.authorization/policydefinitions/{definitionName}?api-version={policyApiVersion}";
                return await ArmHttpHelper.Put(requestUrl, policy, thisOperationContext);
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public async Task<string> DeletePolicy(string subscriptionId, string definitionName, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "PoliciesHelper:DeletePolicy");
            try
            {
                var requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/Microsoft.authorization/policydefinitions/{definitionName}?api-version={policyApiVersion}";
                return await ArmHttpHelper.Delete(requestUrl, thisOperationContext);
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public async Task<string> GetPolicyAssignments(string subscriptionId, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "PoliciesHelper:GetPolicyAssignments");
            try
            {
                var requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/Microsoft.authorization/policyassignments?api-version={policyApiVersion}";
                return await ArmHttpHelper.Get(requestUrl, thisOperationContext);
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public async Task<string> GetPolicyAssignment(string subscriptionId, string policyAssignmentName, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "PoliciesHelper:GetPolicyAssignment");
            try
            {
                var requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/Microsoft.authorization/policyassignments/{policyAssignmentName}?api-version={policyApiVersion}";
                return await ArmHttpHelper.Get(requestUrl, thisOperationContext);
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public async Task<string> SavePolicyAssignment(string subscriptionId, string policyAssignmentName, object policy, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "PoliciesHelper:SavePolicyAssignment");
            try
            {
                var requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/Microsoft.authorization/policyassignments/{policyAssignmentName}?api-version={policyApiVersion}";
                return await ArmHttpHelper.Put(requestUrl, policy, thisOperationContext);
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public async Task<string> DeletePolicyAssignment(string subscriptionId, string policyAssignmentName, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "PoliciesHelper:DeletePolicyAssignment");
            try
            {
                var requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/Microsoft.authorization/policyassignments/{policyAssignmentName}?api-version={policyApiVersion}";
                return await ArmHttpHelper.Delete(requestUrl, thisOperationContext);
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }
    }
}