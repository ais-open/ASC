using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureServiceCatalog.Web.Models.Billing
{
    public class DirectCost : BaseCost, ICost
    {
        protected override double CalculateCosts()
        {
            var cost = Meter.MeterRates.First().Value * BillableQuantity;
            return cost;
        }
    }
}