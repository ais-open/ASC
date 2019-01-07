using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureServiceCatalog.Web.Models.Billing
{
    public enum DataAggregationType
    {
        /// <summary>
        /// All the data across days is aggregated by the resource name
        /// </summary>
        FullAggregationByResource,
        /// <summary>
        /// Data is aggregated on a daily basis by the resource name
        /// </summary>
        DailyAggregationByResource
    }
}