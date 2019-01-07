using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureServiceCatalog.Web.Models.Billing
{
    public class ChartData
    {
        public ChartData()
        {
            Values = new List<XYValue>();
        }
        public string Key { get; set; }
        public List<XYValue> Values { get; internal set; }
    }

    public class XYValue
    {
        public double X { get; set; }
        public double Y { get; set; }
    }
}