using AzureServiceCatalog.Models;
using Microsoft.Azure.Management.Resources.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AzureServiceCatalog.Helpers
{
    public class ConsumptionRepository
    {
        private const string consumptionApiVersion = "2019-05-01";        

        public async Task<List<ResourceUsageDetails>> GetConsumptionUsagesDetails(ResourceListResult resourceList, string subscriptionId, CostEstimationPeriod estimationPeriod, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "ConsumptionRepository:GetConsumptionUsagesDetails");
            try
            {
                List<ResourceUsageDetails> resourceConsumptionDetails = new List<ResourceUsageDetails>();
                var consumptionUsageDetails = await GetConsumptionUsageDetailByScope(subscriptionId, estimationPeriod, thisOperationContext);

                if(estimationPeriod == CostEstimationPeriod.For30Days)
                {
                    resourceConsumptionDetails = GetUsageMonthlyDataByResources(resourceList.Resources, consumptionUsageDetails, thisOperationContext);
                }
                else
                {
                    resourceConsumptionDetails = GetUsageDailyDataByResources(resourceList.Resources, consumptionUsageDetails, thisOperationContext);
                }
                

                return resourceConsumptionDetails;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        private async Task<ConsumptionUsageDetails> GetConsumptionUsageDetailByScope(string subscriptionId, CostEstimationPeriod estimationPeriod, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "ConsumptionRepository:GetConsumptionUsageDetailByScope");

            try
            {
                string requestUrl = null;
                if(estimationPeriod == CostEstimationPeriod.For30Days)
                {
                    DateTime dateTime = DateTime.UtcNow;
                    string  dateTimeTodayAtMidnight = dateTime.ToString("yyyy-MM-ddT00:00:00.0000000Z");
                    string dateimeAt30DaysBeforeMidnight = dateTime.AddDays(-30).ToString("yyyy-MM-ddT00:00:00.0000000Z");

                    requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/Microsoft.Consumption/usageDetails?$filter=properties/usageStart ge '{dateimeAt30DaysBeforeMidnight}' and properties/usageEnd le '{dateTimeTodayAtMidnight}'&api-version={consumptionApiVersion}";
                }
                
                if ( estimationPeriod == CostEstimationPeriod.Today)
                {
                    requestUrl = $"{Config.AzureResourceManagerUrl}/subscriptions/{subscriptionId}/providers/Microsoft.Consumption/usageDetails?api-version={consumptionApiVersion}";
                }
                
                var httpClient = Helpers.GetAuthenticatedHttpClientForUser(thisOperationContext);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                HttpResponseMessage response = await httpClient.SendAsync(request);
                var result = await response.Content.ReadAsStringAsync();
                var resourceUsageList = JsonConvert.DeserializeObject<ConsumptionUsageDetails>(result);
                return resourceUsageList;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }



        public List<ResourceUsageDetails> GetUsageMonthlyDataByResources(IList<GenericResourceExtended> resources, ConsumptionUsageDetails consumptionAggregatesByRG, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "ConsumptionRepository:GetUsageSummaryDataByResources");
            try
            {
                var usageData = new List<ResourceUsageDetails>();
                foreach (var resource in resources)
                {
                    usageData.AddRange(GetUsageMonthlySummaryByResource(resource, consumptionAggregatesByRG, thisOperationContext));
                }
                return usageData;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        private List<ResourceUsageDetails> GetUsageMonthlySummaryByResource(GenericResourceExtended resource, ConsumptionUsageDetails usagePayLoad, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "ConsumptionRepository:GetUsageSummaryByResource");
            try
            {
                var resourceUsageList = new List<ResourceUsageDetails>();
                var usageAggList = usagePayLoad.Value.Where(x => x.Properties != null && x.Properties.cost != 0 && x.Properties.resourceName.Equals(resource.Name, StringComparison.OrdinalIgnoreCase)).ToList();

                if (usageAggList != null && usageAggList.Count != 0)
                {
                    ResourceUsageDetails usage = new ResourceUsageDetails();

                    foreach (var usageItem in usageAggList)
                    {
                        usage.ResourceName = resource.Name;
                        usage.Type = resource.Type;
                        usage.CostForLast30Days += usageItem.Properties.cost;
                        usage.Quantity += usageItem.Properties.quantity;
                        usage.Location = resource.Location;
                        usage.UsageDate = Helpers.ParseDateUtc(usageItem.Properties.date.ToString());
                    }
                    resourceUsageList.Add(usage);
                }
                else
                {
                    ResourceUsageDetails usage = new ResourceUsageDetails();

                    usage.ResourceName = resource.Name;
                    usage.Type = resource.Type;
                    usage.Location = resource.Location;
                    resourceUsageList.Add(usage);
                }

                return resourceUsageList;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public List<ResourceUsageDetails> GetUsageDailyDataByResources(IList<GenericResourceExtended> resources, ConsumptionUsageDetails consumptionAggregatesByRG, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "ConsumptionRepository:GetUsageDailyDataByResources");
            try
            {
                var usageData = new List<ResourceUsageDetails>();
                foreach (var resource in resources)
                {
                    usageData.AddRange(GetUsageDailyByResource(resource, consumptionAggregatesByRG, thisOperationContext));
                }
                return usageData;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }


        private List<ResourceUsageDetails> GetUsageDailyByResource(GenericResourceExtended resource, ConsumptionUsageDetails usagePayLoad, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "ConsumptionRepository:GetUsageDailyByResource");
            try
            {
                var resourceUsageList = new List<ResourceUsageDetails>();
                var usageAggList = usagePayLoad.Value.Where(x => x.Properties != null && x.Properties.cost != 0 && x.Properties.resourceName.Equals(resource.Name, StringComparison.OrdinalIgnoreCase)).ToList();

                if (usageAggList != null && usageAggList.Count != 0)
                {
                    ResourceUsageDetails usage = new ResourceUsageDetails();

                    foreach (var usageItem in usageAggList)
                    {
                        usage.ResourceName = resource.Name;
                        usage.Type = resource.Type;
                        usage.TodaysCost += usageItem.Properties.cost;
                        usage.Quantity += usageItem.Properties.quantity;
                        usage.Location = resource.Location;
                        usage.UsageDate = Helpers.ParseDateUtc(usageItem.Properties.date.ToString());
                    }
                    resourceUsageList.Add(usage);
                }
                else
                {
                    ResourceUsageDetails usage = new ResourceUsageDetails();

                    usage.ResourceName = resource.Name;
                    usage.Type = resource.Type;
                    usage.Location = resource.Location;
                    resourceUsageList.Add(usage);
                }

                return resourceUsageList;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }
    }
}