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
    public class ManagedIdentityHelper
    {
        private const string managedIdentityApiVersion = "2018-11-30";

        public async Task<string> GetUserAssignedIdentities(string subscriptionId, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "ManagedIdentityHelper:GetUserAssignedIdentities");
            try
            {
                var requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/Microsoft.ManagedIdentity/userAssignedIdentities?api-version={managedIdentityApiVersion}";
                return await ArmHttpHelper.Get(requestUrl, thisOperationContext);
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }
    }
}