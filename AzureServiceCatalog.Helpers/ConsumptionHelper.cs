using AzureServiceCatalog.Helpers.Billing;
using AzureServiceCatalog.Helpers.ConsumptionAPI;
using AzureServiceCatalog.Models;
using AzureServiceCatalog.Models.Billing;
using Microsoft.Azure.Management.Resources;
using Microsoft.Azure.Management.Resources.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AzureServiceCatalog.Helpers
{
    public class ConsumptionHelper
    {

        ConsumptionRepository consumptionRepository = new ConsumptionRepository();
        private const string consumptionApiVersion = "2019-05-01";

        public async Task<List<ResourceUsageDetails>> GetConsumptionUsageDetailsForLast30Days(ResourceListResult resourceList, string subscriptionId, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "ConsumptionRepository:GetConsumptionUsageDetailsForLast30Days");

            try
            {
                return await consumptionRepository.GetConsumptionUsagesDetails(resourceList, subscriptionId, CostEstimationPeriod.For30Days, thisOperationContext);
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public async Task<List<ResourceUsageDetails>> GetConsumptionUsageDetailsForToday(ResourceListResult resourceList, string subscriptionId, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "ConsumptionRepository:GetConsumptionUsageDetailsForToday");

            try
            {
                return await consumptionRepository.GetConsumptionUsagesDetails(resourceList, subscriptionId, CostEstimationPeriod.Today, thisOperationContext);
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public List<ChartData> GetChartData(List<ResourceUsageDetails> resourceUsageDataFor30Days, List<ResourceUsageDetails> resourceUsageDataToday, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "ConsumptionHelper:GetChartData");

            try
            {
                return CalculateCostsForChart(resourceUsageDataFor30Days, resourceUsageDataToday, thisOperationContext);
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }

        }

        public static List<ChartData> CalculateCostsForChart(List<ResourceUsageDetails> resourceUsageHistoricalData, List<ResourceUsageDetails> resourceUsageTodaysData, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "ConsumptionHelper:CalculateCostsForChart");
            try
            {
                ConsumptionAPICostStrategyContext costStrategy = new ConsumptionAPICostStrategyContext(resourceUsageHistoricalData, resourceUsageTodaysData);
                return costStrategy.CalculateCostsForChart(thisOperationContext);
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public List<ResourceUsageDetails> GetUsage(List<ResourceUsageDetails> resourceUsageMonthData, List<ResourceUsageDetails> resourceUsageTodayData, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "ConsumptionHelper:GetUsage");
            try
            {

                return CalculateCosts(resourceUsageMonthData, resourceUsageTodayData, thisOperationContext);                
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public static List<ResourceUsageDetails> CalculateCosts(List<ResourceUsageDetails> resourceUsageMonthData, List<ResourceUsageDetails> resourceUsageTodayData, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "ConsumptionHelper:CalculateCosts");
            try
            {
                ConsumptionAPICostStrategyContext costStrategy = new ConsumptionAPICostStrategyContext(resourceUsageMonthData, resourceUsageTodayData);
                return costStrategy.CalculateCosts(thisOperationContext);
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }
    }
}
