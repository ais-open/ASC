using AzureServiceCatalog.Helpers.Billing;
using AzureServiceCatalog.Helpers.ConsumptionAPI;
using AzureServiceCatalog.Models;
using AzureServiceCatalog.Models.Billing;
using Microsoft.Azure.Management.Resources;
using Microsoft.Azure.Management.Resources.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AzureServiceCatalog.Helpers
{
    public class ConsumptionHelper
    {

        ConsumptionRepository consumptionRepository = new ConsumptionRepository();

        public async Task<List<ResourceUsageDetails>> GetUsage(ResourceListResult resourceList, string subscriptionId, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "ConsumptionRepository:GetUsage");
            try
            {
                if (!resourceList.Resources.Any())
                {
                    return new List<ResourceUsageDetails>();
                }

                List<ResourceUsageDetails> resourceUsageMonthData = await GetConsumptionUsageDetailsForLast30Days(resourceList, subscriptionId, thisOperationContext);
                List<ResourceUsageDetails> resourceUsageTodayData = await GetConsumptionUsageDetailsForToday(resourceList, subscriptionId, thisOperationContext);

                return CalculateCosts(resourceUsageMonthData, resourceUsageTodayData, thisOperationContext);
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

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


        public async Task<List<ChartData>> GetChartData(ResourceListResult resourceList, string subscriptionId, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "BillingHelper:GetChartData");
            try
            {
                if (!resourceList.Resources.Any())
                {
                    return new List<ChartData>();
                }

                DateTime dateTime = DateTime.UtcNow;
                string dateTimeTodayAtMidnightAsString = dateTime.ToString("yyyy-MM-ddT00:00:00.0000000Z");
                DateTime dateTimeTodayAtMidnight = DateTime.Parse(dateTimeTodayAtMidnightAsString).Date;

                var resourceUsageData = ConvertConsumptionAggregateToResourceUsageDetails(resourceList.Resources, consumptionRepository.consumptionUsageMonthly.Value, CostEstimationPeriod.For30Days);

                var resourceUsageForToday = consumptionRepository.consumptionUsageToday.Value.Where(x => x.Properties.date == dateTimeTodayAtMidnight).ToList();
                var resourceUsageDataToday = ConvertConsumptionAggregateToResourceUsageDetails(resourceList.Resources, resourceUsageForToday, CostEstimationPeriod.Today);

                
                return CalculateCostsForChart(resourceUsageData, resourceUsageDataToday, thisOperationContext);
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        private List<ResourceUsageDetails> ConvertConsumptionAggregateToResourceUsageDetails(IList<GenericResourceExtended> resources, List<ConsumptionAggregate> consumptionAggregates, CostEstimationPeriod estimationPeriod)
        {
            List<ResourceUsageDetails> resourceUsageDetailsList = new List<ResourceUsageDetails>();

            var usageData = new List<ResourceUsageDetails>();
            foreach (var resource in resources)
            {
                var resourceUsageDetailsByResource = consumptionAggregates.Where(x => x.Properties.resourceName.Equals(resource.Name, StringComparison.OrdinalIgnoreCase)).ToList();

                resourceUsageDetailsList.AddRange(ConvertConsumptionAggregateToResourceUsageDetails(resourceUsageDetailsByResource, estimationPeriod));
            }

            return resourceUsageDetailsList;
        }

        private List<ResourceUsageDetails> ConvertConsumptionAggregateToResourceUsageDetails(List<ConsumptionAggregate> consumptionAggregates, CostEstimationPeriod estimationPeriod)
        {
            List<ResourceUsageDetails> resourceUsageDetailsList = new List<ResourceUsageDetails>();

            foreach (var resource in consumptionAggregates)
            {
                ResourceUsageDetails resourceUsageDetails = new ResourceUsageDetails();
                
                resourceUsageDetails.Location = resource.Properties.resourceLocation;
                resourceUsageDetails.MeterId = resource.Properties.meterId;
                resourceUsageDetails.Quantity = resource.Properties.quantity;
                resourceUsageDetails.ResourceName = resource.Properties.resourceName;
                resourceUsageDetails.Type = resource.Type;
                resourceUsageDetails.UsageDate = resource.Properties.date;
                if(estimationPeriod == CostEstimationPeriod.For30Days)
                {
                    resourceUsageDetails.CostForLast30Days = resource.Properties.cost;
                    resourceUsageDetails.CostForLast30DaysFormatted = null;
                }
                else
                {
                    resourceUsageDetails.TodaysCost = resource.Properties.cost;
                    resourceUsageDetails.TodaysCostFormatted = null;
                }


                resourceUsageDetailsList.Add(resourceUsageDetails);
            }

            return resourceUsageDetailsList;
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
