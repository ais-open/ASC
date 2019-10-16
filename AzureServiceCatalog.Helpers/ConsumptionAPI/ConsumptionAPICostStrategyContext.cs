using AzureServiceCatalog.Models;
using AzureServiceCatalog.Models.Billing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureServiceCatalog.Helpers.ConsumptionAPI
{
    public class ConsumptionAPICostStrategyContext
    {
        private List<ResourceUsageDetails> resourceUsageHistoricalData;
        private List<ResourceUsageDetails> resourceUsageTodaysData;

        public const string StorageAccountType = "Microsoft.Storage/storageAccounts";

        public ConsumptionAPICostStrategyContext(List<ResourceUsageDetails> resourceUsageHistoricalData, List<ResourceUsageDetails> resourceUsageTodaysData)
        {
            this.resourceUsageHistoricalData = resourceUsageHistoricalData;
            this.resourceUsageTodaysData = resourceUsageTodaysData;
        }

        public List<ResourceUsageDetails> CalculateCosts(BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "CostStrategyContext:CalculateCosts");
            try
            {
                //Combine Historical and Todays costs
                CombineCosts(thisOperationContext);

                return SummarizeCostsByResource(thisOperationContext);
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public List<ChartData> CalculateCostsForChart(BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "CostStrategyContext:CalculateCostsForChart");
            try
            {
                //Combine Historical and Todays costs
                if (resourceUsageTodaysData != null && resourceUsageTodaysData.Count > 0)
                {
                    resourceUsageHistoricalData.AddRange(resourceUsageTodaysData);
                }

                return SummarizeDataForChart(thisOperationContext);
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        private void CombineCosts(BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "CostStrategyContext:CombineCosts");
            try
            {
                //Combine the costs
                foreach (var resource in resourceUsageHistoricalData)
                {
                    var resourceToday = resourceUsageTodaysData.Where(m => m.ResourceName == resource.ResourceName).FirstOrDefault();
                    if (resourceToday != null)
                    {
                        resource.TodaysCost = resourceToday.TodaysCost;
                    }
                    resource.FormatCosts();
                }
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }

        }

        /// <summary>
        /// A resource like Storage can have multiple meters assocaited with it. This function sums up all the costs by ResourceName
        /// </summary>
        /// <returns></returns>
        private List<ResourceUsageDetails> SummarizeCostsByResource(BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "CostStrategyContext:SummarizeCostsByResource");
            try
            {
                List<ResourceUsageDetails> resourceCostSummaryList = new List<ResourceUsageDetails>();
                if (resourceUsageHistoricalData == null || resourceUsageHistoricalData.Count == 0)
                {
                    return resourceCostSummaryList;
                }

                List<string> distinctResourceNames = resourceUsageHistoricalData.Select(x => x.ResourceName).Distinct().OrderBy(x => x).ToList();
                foreach (var resourceName in distinctResourceNames)
                {
                    var resource = resourceUsageHistoricalData.FirstOrDefault(x => x.ResourceName == resourceName);

                    var resourceCostSummary = new ResourceUsageDetails();
                    resourceCostSummary.ResourceName = resource.ResourceName;
                    resourceCostSummary.ConsumedService = resource.ConsumedService;
                    resourceCostSummary.Location = resource.Location;
                    resourceCostSummary.Type = resource.Type;
                    resourceCostSummary.CostForLast30Days = resourceUsageHistoricalData.Where(x => x.ResourceName == resourceName).Sum(x => x.CostForLast30Days);
                    resourceCostSummary.TodaysCost = resourceUsageHistoricalData.Where(x => x.ResourceName == resourceName).Sum(x => x.TodaysCost);
                    if (resource.CostForLast30DaysFormatted == ResourceUsageDetails.NotApplicable)
                    {
                        resourceCostSummary.FormatCostsAsNotApplicable();
                    }
                    else
                    {
                        resourceCostSummary.FormatCostsAsCurrency();
                    }
                    resourceCostSummaryList.Add(resourceCostSummary);
                }
                return resourceCostSummaryList;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }

        }

        private List<ChartData> SummarizeDataForChart(BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "CostStrategyContext:SummarizeDataForChart");
            try
            {
                var chartDataList = new List<ChartData>();
                List<string> distinctResourceNames = resourceUsageHistoricalData.Select(x => x.ResourceName).Distinct().OrderBy(x => x).ToList();

                foreach (var resourceName in distinctResourceNames)
                {
                    var resource = resourceUsageHistoricalData.Where(x => x.ResourceName == resourceName).Select(x => x.UsageDate).Distinct();
                    var chartData = new ChartData();
                    chartData.Key = resourceName;

                    var resourceDailyCostSummary = resourceUsageHistoricalData.Where(r => r.ResourceName == resourceName && r.UsageDate.HasValue).GroupBy(r => r.UsageDate, (key, values) => new { UsageDate = key, Cost = values.Sum(x => x.CostForLast30Days)}).OrderBy(x => x.UsageDate);

                    if (resourceDailyCostSummary != null)
                    {
                        foreach (var dailyCostEntry in resourceDailyCostSummary)
                        {

                            chartData.Values.Add(new XYValue { X = dailyCostEntry.UsageDate.Value.ToJavaScriptTicks(), Y = Convert.ToDouble(dailyCostEntry.Cost) });
                        }
                    }

                    chartDataList.Add(chartData);
                }

                return chartDataList;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }
    }
}
