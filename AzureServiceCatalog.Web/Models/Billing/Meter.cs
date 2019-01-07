using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AzureServiceCatalog.Web.Models.Billing
{
    public class Meter
    {
        public Meter()
        {
            MeterRates = new Dictionary<string, double>();
        }

        #region Mapped
        public string MeterId { get; set; }
        public string MeterName { get; set; }
        public string MeterCategory { get; set; }
        public string MeterSubCategory { get; set; }
        public string Unit { get; set; }
        public string MeterRegion { get; set; }
        public Dictionary<string, double> MeterRates { get; internal set; }
        public DateTime EffectiveDate { get; set; }
        public double IncludedQuantity { get; set; }
        #endregion


    }
}
