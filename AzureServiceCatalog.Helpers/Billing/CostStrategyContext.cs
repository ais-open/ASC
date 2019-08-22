using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AzureServiceCatalog.Models;
using AzureServiceCatalog.Models.Billing;

namespace AzureServiceCatalog.Helpers.Billing
{
    public class CostStrategyContext
    {
        private List<ResourceUsage> resourceUsageHistoricalData;
        private List<ResourceUsage> resourceUsageTodaysData;
        private RateCard rateCard;

        public const string StorageAccountType = "Microsoft.Storage/storageAccounts";

        public CostStrategyContext(List<ResourceUsage> resourceUsageHistoricalData, List<ResourceUsage> resourceUsageTodaysData, RateCard rateCard)
        {
            this.resourceUsageHistoricalData = resourceUsageHistoricalData;
            this.resourceUsageTodaysData = resourceUsageTodaysData;
            this.rateCard = rateCard;
        }

        public List<ResourceUsage> CalculateCosts(BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "CostStrategyContext:CalculateCosts");
            try
            {
                //Calculate Historical Costs
                CalculateCosts(false, thisOperationContext);

                //Calculate Todays Costs
                CalculateCosts(true, thisOperationContext);

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
                //Calculate Historical Costs
                CalculateCosts(resourceUsageHistoricalData, false, thisOperationContext);

                //Calculate Todays Costs
                CalculateCosts(resourceUsageTodaysData, false, thisOperationContext);

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

        private void CalculateCosts(bool isTodaysCost, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "CostStrategyContext:CalculateCosts");
            try
            {
                List<ResourceUsage> usageData = (isTodaysCost ? resourceUsageTodaysData : resourceUsageHistoricalData);

                CalculateCosts(usageData, isTodaysCost, thisOperationContext);
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }

        }

        private void CalculateCosts(List<ResourceUsage> usageData, bool isTodaysCost, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "CostStrategyContext:CalculateCosts");
            try
            {
                if (rateCard == null || rateCard.Meters == null)
                {
                    return;
                }

                foreach (var resource in usageData)
                {
                    var meter = rateCard.Meters.Where(m => m.MeterId == resource.MeterId).FirstOrDefault();
                    if (meter != null)
                    {
                        if (isTodaysCost)
                        {
                            resource.TodaysCost = GetCost(resource, meter, thisOperationContext);
                        }
                        else
                        {
                            resource.CostForLast30Days = GetCost(resource, meter, thisOperationContext);
                        }
                    }
                }
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
            
        }

        private static double GetCost(ResourceUsage resource, Meter meter, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "CostStrategyContext:GetCost");
            try
            {
                ICost costImplementation = null;

                if (meter.MeterRates.Count() == 1)
                {
                    costImplementation = new DirectCost();
                }
                else if (meter.MeterRates.Count() > 1)
                {
                    if (resource.Type == StorageAccountType)
                    {
                        costImplementation = new BracketCost();
                    }
                    else
                    {
                        costImplementation = new ScaledCost();
                    }
                }

                return costImplementation.CalculateCosts(resource, meter);
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
        private List<ResourceUsage> SummarizeCostsByResource(BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "CostStrategyContext:SummarizeCostsByResource");
            try
            {
                List<ResourceUsage> resourceCostSummaryList = new List<ResourceUsage>();
                if (resourceUsageHistoricalData == null || resourceUsageHistoricalData.Count == 0)
                {
                    return resourceCostSummaryList;
                }

                List<string> distinctResourceNames = resourceUsageHistoricalData.Select(x => x.ResourceName).Distinct().OrderBy(x => x).ToList();
                foreach (var resourceName in distinctResourceNames)
                {
                    var resource = resourceUsageHistoricalData.FirstOrDefault(x => x.ResourceName == resourceName);

                    var resourceCostSummary = new ResourceUsage();
                    resourceCostSummary.ResourceName = resource.ResourceName;
                    resourceCostSummary.Type = resource.Type;
                    resourceCostSummary.Location = resource.Location;
                    resourceCostSummary.CostForLast30Days = resourceUsageHistoricalData.Where(x => x.ResourceName == resourceName).Sum(x => x.CostForLast30Days);
                    resourceCostSummary.TodaysCost = resourceUsageHistoricalData.Where(x => x.ResourceName == resourceName).Sum(x => x.TodaysCost);
                    if (resource.CostForLast30DaysFormatted == ResourceUsage.NotApplicable)
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
                    var resource = resourceUsageHistoricalData.FirstOrDefault(x => x.ResourceName == resourceName);
                    var chartData = new ChartData();
                    chartData.Key = resourceName;

                    var resourceDailyCostSummary = resourceUsageHistoricalData.Where(r => r.ResourceName == resourceName && r.UsageDate.HasValue).GroupBy(r => r.UsageDate, (key, values) => new { UsageDate = key, Cost = values.Sum(x => x.CostForLast30Days) });

                    if (resourceDailyCostSummary != null)
                    {
                        foreach (var dailyCostEntry in resourceDailyCostSummary)
                        {

                            chartData.Values.Add(new XYValue { X = dailyCostEntry.UsageDate.Value.ToJavaScriptTicks(), Y = dailyCostEntry.Cost });
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