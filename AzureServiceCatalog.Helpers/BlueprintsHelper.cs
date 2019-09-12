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
    public class BlueprintsHelper
    {
        private const string blueprintApiVersion = "2018-11-01-preview";

        public async Task<string> GetBlueprintDefinitions(string subscriptionId, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "BlueprintsHelper:GetBlueprintDefinitions");
            try
            {
                var requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/Microsoft.Blueprint/blueprints?api-version={blueprintApiVersion}";
                return await ArmHttpHelper.Get(requestUrl, thisOperationContext);
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public async Task<string> GetBlueprintVersions(string subscriptionId, string blueprintName, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "BlueprintsHelper:GetBlueprintVersions");
            try
            {
                var requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/Microsoft.Blueprint/blueprints/{blueprintName}/versions?api-version={blueprintApiVersion}";
                return await ArmHttpHelper.Get(requestUrl, thisOperationContext);
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public async Task<string> GetBlueprintAssignments(string subscriptionId, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "BlueprintsHelper:GetBlueprintAssignments");
            try
            {
                var requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/Microsoft.Blueprint/blueprintAssignments?api-version={blueprintApiVersion}";
                return await ArmHttpHelper.Get(requestUrl, thisOperationContext);
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public async Task<string> GetAssignedBlueprint(string subscriptionId, string blueprintAssignmentName, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "BlueprintsHelper:GetAssignedBlueprint");
            try
            {
                var requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/Microsoft.Blueprint/blueprintAssignments/{blueprintAssignmentName}?api-version={blueprintApiVersion}";
                return await ArmHttpHelper.Get(requestUrl, thisOperationContext);
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public async Task<string> AssignBlueprint(string subscriptionId, string assignmentName, object blueprintAssignment, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "BlueprintsHelper:AssignBlueprint");
            try
            {
                var requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/Microsoft.Blueprint/blueprintAssignments/{assignmentName}?api-version={blueprintApiVersion}";
                return await ArmHttpHelper.Put(requestUrl, blueprintAssignment, thisOperationContext);
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public async Task<string> GetObjectIdOfBlueprintServicePrincipal(string subscriptionId, string blueprintAssignmentName, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "BlueprintsHelper:GetObjectIdOfBlueprintServicePrincipal");
            try
            {
                var requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/Microsoft.Blueprint/blueprintAssignments/{blueprintAssignmentName}/WhoIsBlueprint?api-version={blueprintApiVersion}";
                return await ArmHttpHelper.Post(requestUrl, null, thisOperationContext);
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }

        }
    }
}