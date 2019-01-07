using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureServiceCatalog.Web.Models.Billing
{
    public class BaseFilterParameters
    {
        public const string DefaultApiVersion = "2015-06-01-preview";
        public string ApiVersion { get; set; }
        public string SubscriptionId { get; set; }

        public BaseFilterParameters()
        {
            ApiVersion = DefaultApiVersion;
        }

        public virtual string GetFormattedFilter()
        {
            throw new NotImplementedException();
        }

        protected virtual string FormatForQueryParameter(DateTime value)
        {
            return value.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}