using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace AzureServiceCatalog.Web.Models
{
    public class ResourceUsage
    {
        public const string NotApplicable = "n/a";
        [JsonProperty("Name")]
        public string ResourceName { get; set; }
        public string Location { get; set; }
        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "Probably shouldn't have been named this - but need to maintain serialization for now.")]
        public string Type { get; set; }
        public double Quantity { get; set; }
        public string MeterId { get; set; }
        public string MeterName { get; internal set; }
        public string MeterCategory { get; set; }
        public string MeterSubCategory { get; set; }
        public int MeterRate { get; set; }
        [JsonProperty("Cost")]
        public double CostForLast30Days { get; set; }
        public double TodaysCost { get; set; }
        [JsonProperty("CostFormatted")]
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
}