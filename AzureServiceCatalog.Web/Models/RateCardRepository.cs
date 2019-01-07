using AzureServiceCatalog.Web.Models.Billing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace AzureServiceCatalog.Web.Models
{
    public class RateCardRepository
    {
        private const string rateCardApiUrlFormat = "https://management.azure.com/subscriptions/{0}/providers/Microsoft.Commerce/RateCard?{1}";
        /// <summary>
        /// The name will include the OfferId. As the pricing can change based on the OfferId.
        /// </summary>
        private const string RateCardCacheNameFormat = "RateCard_{0}";

        public async Task<RateCard> GetRateCardData(string subscriptionId)
        {
            RateCardFilterParameters rateCardFilterParameters = RateCardFilterParameters.GetRateCardFilter();
            rateCardFilterParameters.SubscriptionId = subscriptionId;

            var rateCardData = await GetRateCardData(rateCardFilterParameters);
            return rateCardData;
        }

        public async Task<RateCard> GetRateCardData(RateCardFilterParameters rateCardFilter)
        {
            string rateCardCacheName = string.Format(RateCardCacheNameFormat, rateCardFilter.OfferId);
            RateCard rateCardData = MemoryCacher.GetValue(rateCardCacheName) as RateCard;
            if (rateCardData == null)
            {
                var rateCardResponseData = await RequestRateCardDataFromService(rateCardFilter);
                rateCardData = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<RateCard>(rateCardResponseData));
                //Cache the RateCardData
                MemoryCacher.Add(rateCardCacheName, rateCardData, DateTime.Now.AddHours(1));
            }

            return rateCardData;
        }

        private async Task<string> RequestRateCardDataFromService(RateCardFilterParameters rateCardFilter)
        {
            string rateCardUri = string.Format(rateCardApiUrlFormat,
                rateCardFilter.SubscriptionId,
                rateCardFilter.GetFormattedFilter());

            var client = Utils.GetAuthenticatedHttpClientForApp();

            return await client.GetStringAsync(rateCardUri);
        }
    }
}