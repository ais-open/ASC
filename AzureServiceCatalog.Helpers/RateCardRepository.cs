using AzureServiceCatalog.Models.Billing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using AzureServiceCatalog.Models;

namespace AzureServiceCatalog.Helpers
{
    public class RateCardRepository
    {
        private const string rateCardApiUrlFormat = "https://management.azure.com/subscriptions/{0}/providers/Microsoft.Commerce/RateCard?{1}";
        /// <summary>
        /// The name will include the OfferId. As the pricing can change based on the OfferId.
        /// </summary>
        private const string RateCardCacheNameFormat = "RateCard_{0}";

        public async Task<RateCard> GetRateCardData(string subscriptionId, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "RateCardRepository:GetRateCardData");
            try
            {
                RateCardFilterParameters rateCardFilterParameters = RateCardFilterParameters.GetRateCardFilter();
                rateCardFilterParameters.SubscriptionId = subscriptionId;

                var rateCardData = await GetRateCardData(rateCardFilterParameters, thisOperationContext);
                return rateCardData;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public async Task<RateCard> GetRateCardData(RateCardFilterParameters rateCardFilter, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "RateCardRepository:GetRateCardData");
            try
            {
                string rateCardCacheName = string.Format(RateCardCacheNameFormat, rateCardFilter.OfferId, thisOperationContext);
                RateCard rateCardData = MemoryCacher.GetValue(rateCardCacheName, thisOperationContext) as RateCard;
                if (rateCardData == null)
                {
                    var rateCardResponseData = await RequestRateCardDataFromService(rateCardFilter, thisOperationContext);
                    rateCardData = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<RateCard>(rateCardResponseData));
                    //Cache the RateCardData
                    MemoryCacher.Add(rateCardCacheName, rateCardData, DateTime.Now.AddHours(1), thisOperationContext);
                }

                return rateCardData;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        private async Task<string> RequestRateCardDataFromService(RateCardFilterParameters rateCardFilter, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "RateCardRepository:RequestRateCardDataFromService");
            try
            {
                string rateCardUri = string.Format(rateCardApiUrlFormat,
                    rateCardFilter.SubscriptionId,
                    rateCardFilter.GetFormattedFilter());

                var client = Helpers.GetAuthenticatedHttpClientForApp(thisOperationContext);

                return await client.GetStringAsync(rateCardUri);
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }
    }
}