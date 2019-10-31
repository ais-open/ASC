using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureServiceCatalog.Models
{
    public class ResourceUsageDetails
    {
        public const string NotApplicable = "n/a";
        public string ResourceName { get; set; }
        public string Location { get; set; }
        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "Probably shouldn't have been named this - but need to maintain serialization for now.")]
        public string Type { get; set; }
        public decimal Quantity { get; set; }
        public string ConsumedService { get; set; }
        public decimal EffectivePrice { get; set; }
        public decimal UnitPrice { get; set; }
        public string BillingCurrency { get; set; }
        public string MeterId { get; set; }
        public decimal Cost { get; set; }
        public decimal CostForLast30Days { get; set; }
        public decimal TodaysCost { get; set; }
        public string CostForLast30DaysFormatted { get; set; }
        public string TodaysCostFormatted { get; set; }
        /// <summary>
        /// UsageDate is used only for Daily Usage to show data in charts
        /// </summary>
        public DateTime? UsageDate { get; set; }

        public void FormatCosts()
        {
            if (!string.IsNullOrEmpty(MeterId))
            {
                FormatCostsAsCurrency();
            }
            else
            {
                FormatCostsAsNotApplicable();
            }
        }

        public void FormatCostsAsNotApplicable()
        {
            CostForLast30DaysFormatted = NotApplicable;
            TodaysCostFormatted = NotApplicable;
        }

        public void FormatCostsAsCurrency()
        {
            CostForLast30DaysFormatted = CostForLast30Days.ToString("C");
            TodaysCostFormatted = TodaysCost.ToString("C");
        }
    }

    public enum CostEstimationPeriod
    {
        For30Days = 0,
        Today = 1
    }
}
