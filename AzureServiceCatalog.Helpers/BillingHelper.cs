using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using AzureServiceCatalog.Models;
using AzureServiceCatalog.Models.Billing;
using AzureServiceCatalog.Helpers.Billing;
using Microsoft.Azure.Management.Resources.Models;

namespace AzureServiceCatalog.Helpers
{
    public class BillingHelper
    {
        RateCardRepository rateCardRepository = new RateCardRepository();
        BillingRepository billingRepository = new BillingRepository();
        public async Task<List<ChartData>> GetChartData(ResourceListResult resourceList, string subscriptionId, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "BillingHelper:GetChartData");
            try
            {
                if (!resourceList.Resources.Any())
                {
                    return new List<ChartData>();
                }
                var rateCard = rateCardRepository.GetRateCardData(subscriptionId, thisOperationContext);
                var resourceUsageData = await billingRepository.GetBillingDataForLast30Days(resourceList, subscriptionId, DataAggregationType.DailyAggregationByResource, thisOperationContext);
                var resourceUsageDataToday = await billingRepository.GetBillingDataForToday(resourceList, subscriptionId, DataAggregationType.DailyAggregationByResource, thisOperationContext);
                return CalculateCostsForChart(resourceUsageData, resourceUsageDataToday, rateCard.Result, thisOperationContext);
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }

        }

        public async Task<List<ResourceUsage>> GetUsage(ResourceListResult resourceList, string subscriptionId, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "BillingHelper:GetUsage");
            try
            {
                if (!resourceList.Resources.Any())
                {
                    return new List<ResourceUsage>();
                }

                var rateCard = await rateCardRepository.GetRateCardData(subscriptionId, thisOperationContext);
                var resourceUsageData = await billingRepository.GetBillingDataForLast30Days(resourceList, subscriptionId, DataAggregationType.FullAggregationByResource, thisOperationContext);
   
                var resourceUsageDataToday = await billingRepository.GetBillingDataForToday(resourceList, subscriptionId, DataAggregationType.FullAggregationByResource, thisOperationContext);

                return CalculateCosts(resourceUsageData, resourceUsageDataToday, rateCard, thisOperationContext);
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public static List<ResourceUsage> CalculateCosts(List<ResourceUsage> resourceUsageHistoricalData, List<ResourceUsage> resourceUsageTodaysData, RateCard rateCard, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "BillingHelper:CalculateCosts");
            try
            {
                CostStrategyContext costStrategy = new CostStrategyContext(resourceUsageHistoricalData, resourceUsageTodaysData, rateCard);
                return costStrategy.CalculateCosts(thisOperationContext);
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public static List<ChartData> CalculateCostsForChart(List<ResourceUsage> resourceUsageHistoricalData, List<ResourceUsage> resourceUsageTodaysData, RateCard rateCard, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "BillingHelper:CalculateCostsForChart");
            try
            {
                CostStrategyContext costStrategy = new CostStrategyContext(resourceUsageHistoricalData, resourceUsageTodaysData, rateCard);
                return costStrategy.CalculateCostsForChart(thisOperationContext);
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }
    }
}