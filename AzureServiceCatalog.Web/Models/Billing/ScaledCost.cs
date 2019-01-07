using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureServiceCatalog.Web.Models.Billing
{
    public class ScaledCost : BaseCost, ICost
    {
        protected override double CalculateCosts()
        {
            string keyToUse = GetHighestMeterRateKeyBasedOnQuantity();

            if (string.IsNullOrEmpty(keyToUse))
            {
                keyToUse = Meter.MeterRates.First().Key;
            }

            var cost = Meter.MeterRates[keyToUse] * BillableQuantity;
            return cost;
        }

        private string GetHighestMeterRateKeyBasedOnQuantity()
        {
            string keyToUse = Meter.MeterRates.Keys.Where(k => Utils.ParseInt64(k) <= BillableQuantity).OrderByDescending(k => Utils.ParseInt64(k)).FirstOrDefault();
            return keyToUse;
        }
    }
}