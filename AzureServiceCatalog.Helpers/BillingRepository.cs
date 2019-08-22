using AzureServiceCatalog.Models.Billing;
using Microsoft.Azure.Management.Resources.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using AzureServiceCatalog.Models;

namespace AzureServiceCatalog.Helpers
{
    public class BillingRepository
    {

        private const string billingApiFormat = "https://management.azure.com/subscriptions/{0}/providers/Microsoft.Commerce/UsageAggregates?{1}";

        public async Task<List<ResourceUsage>> GetBillingDataForLast30Days(ResourceListResult resourceList, string subscriptionId, DataAggregationType aggregationType, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "BillingRepository:GetBillingDataForLast30Days");
            try
            {
                UsageFilterParameters usageFilterParameters = UsageFilterParameters.GetFilterParametersForLast30Days();
                usageFilterParameters.SubscriptionId = subscriptionId;

                if (aggregationType == DataAggregationType.FullAggregationByResource)
                    return await GetBillingData(resourceList, usageFilterParameters, thisOperationContext);
                else if (aggregationType == DataAggregationType.DailyAggregationByResource)
                    return await GetBillingDataWithDailyAggregation(resourceList, usageFilterParameters, thisOperationContext);

                return null;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public async Task<List<ResourceUsage>> GetBillingDataForToday(ResourceListResult resourceList, string subscriptionId, DataAggregationType aggregationType, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "BillingRepository:GetBillingDataForToday");
            try
            {
                UsageFilterParameters usageFilterParameters = UsageFilterParameters.GetFilterParametersForToday();
                usageFilterParameters.SubscriptionId = subscriptionId;

                if (aggregationType == DataAggregationType.FullAggregationByResource)
                    return await GetBillingData(resourceList, usageFilterParameters, thisOperationContext);
                else if (aggregationType == DataAggregationType.DailyAggregationByResource)
                    return await GetBillingDataWithDailyAggregation(resourceList, usageFilterParameters, thisOperationContext);

                return null;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        private async Task<List<ResourceUsage>> GetBillingData(ResourceListResult resourceList, UsageFilterParameters usageFilterParameters, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "BillingRepository:GetBillingData");
            try
            {
                var usageData = await GetUsageData(usageFilterParameters, thisOperationContext);

                var resourceUsageData = GetUsageSummaryDataByResources(resourceList.Resources, usageData, thisOperationContext);
                return resourceUsageData;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        private async Task<List<ResourceUsage>> GetBillingDataWithDailyAggregation(ResourceListResult resourceList, UsageFilterParameters usageFilterParameters, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "BillingRepository:GetBillingDataWithDailyAggregation");
            try
            {
                var usageData = await GetUsageData(usageFilterParameters, thisOperationContext);

                var resourceUsageData = GetUsageDailyDataByResources(resourceList.Resources, usageData, thisOperationContext);
                return resourceUsageData;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public List<ResourceUsage> GetUsageSummaryDataByResources(IList<GenericResourceExtended> resources, UsagePayload usagePayLoad, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "BillingRepository:GetUsageSummaryDataByResources");
            try
            {
                var usageData = new List<ResourceUsage>();
                foreach (var resource in resources)
                {
                    usageData.AddRange(GetUsageSummaryByResource(resource, usagePayLoad, thisOperationContext));
                }
                return usageData;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public List<ResourceUsage> GetUsageDailyDataByResources(IList<GenericResourceExtended> resources, UsagePayload usagePayLoad, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "BillingRepository:GetUsageDailyDataByResources");
            try
            {
                var usageData = new List<ResourceUsage>();
                foreach (var resource in resources)
                {
                    usageData.AddRange(GetUsageDailyByResource(resource, usagePayLoad, thisOperationContext));
                }
                return usageData;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public async Task<UsagePayload> GetUsageData(UsageFilterParameters usageFilterParameters, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "BillingRepository:GetUsageData");
            try
            {
                UsagePayload usageData;

                var usageDataResponse = await RequestUsageDataFromService(usageFilterParameters, thisOperationContext);

                if (string.IsNullOrEmpty(usageDataResponse))
                {
                    usageData = new UsagePayload();
                }
                else
                {
                    usageData = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<UsagePayload>(usageDataResponse));
                }
                return usageData;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        private List<ResourceUsage> GetUsageSummaryByResource(GenericResourceExtended resource, UsagePayload usagePayLoad, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "BillingRepository:GetUsageSummaryByResource");
            try
            {
                var resourceUsageList = new List<ResourceUsage>();
                var usageAggList = FindUsageAggregateByResource(resource.Name, usagePayLoad, thisOperationContext);

                if (usageAggList == null || usageAggList.Count == 0)
                {
                    ResourceUsage usage = new ResourceUsage();
                    usage.ResourceName = resource.Name;
                    usage.Type = resource.Type;
                    usage.Location = resource.Location;
                    resourceUsageList.Add(usage);
                    return resourceUsageList;
                }
                else
                {
                    resourceUsageList = GetResourceSummaryByMeter(resource, usageAggList, thisOperationContext);
                }

                return resourceUsageList;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        private List<ResourceUsage> GetResourceSummaryByMeter(GenericResourceExtended resource, List<UsageAggregate> usageAggList, BaseOperationContext parentOperationContext, bool isTodaysData = false)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "BillingRepository:GetResourceSummaryByMeter");
            try
            {
                var resourceUsageList = new List<ResourceUsage>();
                var distinctMeters = usageAggList.Select(x => x.Properties.MeterId).Distinct();
                foreach (var meter in distinctMeters)
                {
                    var firstEntry = usageAggList.Where(m => m.Properties.MeterId == meter).First();
                    ResourceUsage usage = new ResourceUsage();
                    usage.ResourceName = resource.Name;
                    usage.Type = resource.Type;
                    usage.Location = resource.Location;
                    usage.MeterId = meter;
                    usage.MeterCategory = firstEntry.Properties.MeterCategory;
                    usage.MeterSubCategory = firstEntry.Properties.MeterSubCategory;
                    usage.MeterName = firstEntry.Properties.MeterName;
                    usage.Quantity = usageAggList.Where(x => x.Properties.MeterId == meter).Sum(x => x.Properties.Quantity);
                    if (isTodaysData)
                    {
                        usage.UsageDate = Helpers.GetCurrentDateWithMidnightTime();
                    }
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

        private List<ResourceUsage> GetUsageDailyByResource(GenericResourceExtended resource, UsagePayload usagePayLoad, BaseOperationContext parentOperationContext, bool isTodaysData = false)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "BillingRepository:GetUsageDailyByResource");
            try
            {
                var resourceUsageList = new List<ResourceUsage>();
                var usageAggList = FindUsageAggregateByResource(resource.Name, usagePayLoad, thisOperationContext);

                if (usageAggList == null || usageAggList.Count == 0)
                {
                    ResourceUsage usage = new ResourceUsage();
                    usage.ResourceName = resource.Name;
                    usage.Type = resource.Type;
                    usage.Location = resource.Location;
                    resourceUsageList.Add(usage);
                }
                else
                {
                    if (isTodaysData)
                    {
                        //Todays data is hourly data, so the data needs to be summarized by Meters for a resource
                        resourceUsageList = GetResourceSummaryByMeter(resource, usageAggList, thisOperationContext, isTodaysData);
                    }
                    else
                    {
                        foreach (var usageItem in usageAggList)
                        {
                            ResourceUsage usage = new ResourceUsage();
                            usage.ResourceName = resource.Name;
                            usage.Type = resource.Type;
                            usage.Location = resource.Location;
                            usage.MeterId = usageItem.Properties.MeterId;
                            usage.MeterCategory = usageItem.Properties.MeterCategory;
                            usage.MeterSubCategory = usageItem.Properties.MeterSubCategory;
                            usage.MeterName = usageItem.Properties.MeterName;
                            usage.Quantity = usageItem.Properties.Quantity;
                            usage.UsageDate = Helpers.ParseDateUtc(usageItem.Properties.UsageStartTime);
                            resourceUsageList.Add(usage);
                        }
                    }
                }

                return resourceUsageList;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        private List<UsageAggregate> FindUsageAggregateByResource(string resourceName, UsagePayload usagePayLoad, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "BillingRepository:FindUsageAggregateByResource");
            try
            {
                List<UsageAggregate> usage = usagePayLoad.Value.Where(x => x.Properties != null && x.Properties.InfoFields != null && !string.IsNullOrEmpty(x.Properties.InfoFields.Project) && x.Properties.InfoFields.Project.Equals(resourceName, StringComparison.OrdinalIgnoreCase)).ToList();

                if (usage == null || usage.Count == 0)
                    usage = usagePayLoad.Value.Where(x => x.Properties != null && x.Properties.InstanceDataRaw != null && !string.IsNullOrEmpty(x.Properties.InstanceData.MicrosoftResources.ResourceUri) && x.Properties.InstanceData.MicrosoftResources.ResourceUri.EndsWith(resourceName, StringComparison.OrdinalIgnoreCase)).ToList();

                return usage;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        private async Task<string> RequestUsageDataFromService(UsageFilterParameters usageFilterParameters, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "BillingRepository:RequestUsageDataFromService");
            try
            {
                if (!usageFilterParameters.IsValidReportedTime)
                {
                    return string.Empty;
                }

                string billingUri = string.Format(billingApiFormat,
                    usageFilterParameters.SubscriptionId,
                    usageFilterParameters.GetFormattedFilter());

                var client = Helpers.GetAuthenticatedHttpClientForApp(thisOperationContext);
                client.DefaultRequestHeaders.Add(HttpRequestHeader.ContentType.ToString(), "application/json");
                return await client.GetStringAsync(billingUri);
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
                return null;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

    }
}