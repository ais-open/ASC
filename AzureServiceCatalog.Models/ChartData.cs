using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Web;
using Wintellect.Azure.Storage.Table;

namespace AzureServiceCatalog.Models
{

    public class BudgetChartData
    {
        public List<TrendlineData> BudgetData { get; set; }
        public List<TrendlineData> CostData { get; set; }
        public List<DoughnutData> CostDoughnutData { get; set; }
        public string BudgetAmount { get; set; }
        public string TotalCost { get; set; }
        public string Variance { get; set; }
        public string VariancePercentage { get; set; }
        public string DoughnutchartTitle { get; set; }
    }

    public class DoughnutData
    {
        public string xValue { get; set; }
        public double yValue { get; set; }
        public string text { get; set; }
    }

    public class TrendlineData
    {
        public DateTime x { get; set; }
        public double y { get; set; }
    }
}