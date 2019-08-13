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
        public async Task<List<ChartData>> GetChartData(ResourceListResult resourceList, string subscriptionId)
        {
            if (!resourceList.Resources.Any())
            {
                return new List<ChartData>();
            }

            var rateCard = rateCardRepository.GetRateCardData(subscriptionId);

            var resourceUsageData = await billingRepository.GetBillingDataForLast30Days(resourceList, subscriptionId, DataAggregationType.DailyAggregationByResource);

            var resourceUsageDataToday = await billingRepository.GetBillingDataForToday(resourceList, subscriptionId, DataAggregationType.DailyAggregationByResource);

            return CalculateCostsForChart(resourceUsageData, resourceUsageDataToday, rateCard.Result);
        }

        public async Task<List<ResourceUsage>> GetUsage(ResourceListResult resourceList, string subscriptionId)
        {
            if (!resourceList.Resources.Any())
            {
                return new List<ResourceUsage>();
            }

            var rateCard = await rateCardRepository.GetRateCardData(subscriptionId);

            var resourceUsageData = await billingRepository.GetBillingDataForLast30Days(resourceList, subscriptionId, DataAggregationType.FullAggregationByResource);

            var resourceUsageDataToday = await billingRepository.GetBillingDataForToday(resourceList, subscriptionId, DataAggregationType.FullAggregationByResource);

            return CalculateCosts(resourceUsageData, resourceUsageDataToday, rateCard);
        }

        public static List<ResourceUsage> CalculateCosts(List<ResourceUsage> resourceUsageHistoricalData, List<ResourceUsage> resourceUsageTodaysData, RateCard rateCard)
        {
            CostStrategyContext costStrategy = new CostStrategyContext(resourceUsageHistoricalData, resourceUsageTodaysData, rateCard);
            return costStrategy.CalculateCosts();
        }

        public static List<ChartData> CalculateCostsForChart(List<ResourceUsage> resourceUsageHistoricalData, List<ResourceUsage> resourceUsageTodaysData, RateCard rateCard)
        {
            CostStrategyContext costStrategy = new CostStrategyContext(resourceUsageHistoricalData, resourceUsageTodaysData, rateCard);
            return costStrategy.CalculateCostsForChart();
        }
    }
}