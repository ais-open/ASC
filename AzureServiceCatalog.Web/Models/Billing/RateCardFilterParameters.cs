using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureServiceCatalog.Web.Models.Billing
{
    public class RateCardFilterParameters : BaseFilterParameters
    {
        //public const string DefaultOfferId = "MS-AZR-0063P";
        public const string DefaultLocale = "en-US";
        public const string DefaultRegion = "US";
        public const string DefaultCurrency = "USD";
        private const string rateCardFilterFormat = "api-version={0}&$filter=OfferDurableId eq '{1}' and Currency eq '{2}' and Locale eq '{3}' and RegionInfo eq '{4}'";

        /// <summary>
        /// TODO - We may have to either allow user to choose an OfferId or get it from their subscription
        /// </summary>
        public string OfferId { get; set; }
        public string Currency { get; set; }
        public string Locale { get; set; }
        public string Region { get; set; }

        public RateCardFilterParameters(string offerId) : base()
        {
            OfferId = offerId;
            Currency = DefaultCurrency;
            Locale = DefaultLocale;
            Region = DefaultRegion; 
        }

        public override string GetFormattedFilter()
        {
            return string.Format(rateCardFilterFormat, ApiVersion, OfferId, Currency, Locale, Region);
        }

        public static RateCardFilterParameters GetRateCardFilter()
        {
            RateCardFilterParameters rateCardFilterParameters = new RateCardFilterParameters(Config.RateCardOfferId);
            return rateCardFilterParameters;
        }

    }
}