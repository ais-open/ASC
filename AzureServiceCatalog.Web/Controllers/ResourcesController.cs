using Microsoft.Azure.Management.Resources;
using Microsoft.Azure.Management.Resources.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using AzureServiceCatalog.Models.Billing;
using AzureServiceCatalog.Helpers.Billing;
using AzureServiceCatalog.Models;
using AzureServiceCatalog.Helpers;

namespace AzureServiceCatalog.Web.Controllers
{
    public class ResourcesController : ApiController
    {
        TableCoreRepository coreRepository = new TableCoreRepository();
        RateCardRepository rateCardRepository = new RateCardRepository();
        BillingRepository billingRepository = new BillingRepository();

        [Route("api/subscriptions/{subscriptionId}/resourceGroups")]
        public async Task<HttpResponseMessage> GetResourceGroupsBySubscription(string subscriptionId)
        {
                var json = await AzureResourceManagerUtil.GetUserResourceGroups(subscriptionId);
                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                response.Content = json.ToStringContent();
                return response;
          
        }

        [Route("api/subscriptions/{subscriptionId}/resourceGroupsAll")]
        public async Task<IHttpActionResult> GetAllResourceGroupsBySubscription(string subscriptionId)
        {
                var client = Utils.GetResourceManagementClient(subscriptionId);
                var list = await client.ResourceGroups.ListAsync(new ResourceGroupListParameters());
                return this.Ok(list);
        }

        [Route("api/subscriptions/{subscriptionId}/user-resource-groups")]
        public async Task<HttpResponseMessage> GetAllUserResourceGroupsBySubscription(string subscriptionId)
        {
            var json = await AzureResourceManagerUtil.GetUserResourceGroups(subscriptionId);
            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = json.ToStringContent();
            return response;
        }

        [Route("api/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}")]
        public async Task<IHttpActionResult> GetResourceGroupResources(string resourceGroupName, string subscriptionId)
        {
                var client = Utils.GetResourceManagementClient(subscriptionId);
                var resourceList = await client.Resources.ListAsync(new ResourceListParameters { ResourceGroupName = resourceGroupName });

                List<ResourceUsage> resourceUsageData = await GetUsage(resourceList, subscriptionId);

                return this.Ok(resourceUsageData);
          
        }

        [Route("api/subscriptions/{subscriptionId}/chartData/{resourceGroupName}")]
        public async Task<IHttpActionResult> GetChartData(string resourceGroupName, string subscriptionId)
        {
            try
            {
                var client = Utils.GetResourceManagementClient(subscriptionId);
                var resourceList = await client.Resources.ListAsync(new ResourceListParameters { ResourceGroupName = resourceGroupName });

                var chartData = await GetChartData(resourceList, subscriptionId);

                return this.Ok(chartData);
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
                return InternalServerError(ex);
            }
        }

        [Route("api/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/full")]
        public async Task<IHttpActionResult> GetResourceGroupData(string resourceGroupName, string subscriptionId)
        {
                var client = Utils.GetResourceManagementClient(subscriptionId);
                var resourceList = await client.Resources.ListAsync(new ResourceListParameters { ResourceGroupName = resourceGroupName });

                var resourceUsageData = await GetUsage(resourceList, subscriptionId);
                var chartData = await GetChartData(resourceList, subscriptionId);

                var rgData = new ResourceGroupData
                {
                    ResourceGroupName = resourceGroupName,
                    ResourcesUsages = resourceUsageData,
                    ChartData = chartData
                };
                return this.Ok(rgData);
          
        }

        private async  Task<List<ChartData>> GetChartData(ResourceListResult resourceList, string subscriptionId)
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

        private async Task<List<ResourceUsage>> GetUsage(ResourceListResult resourceList, string subscriptionId)
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

        private static List<ResourceUsage> CalculateCosts(List<ResourceUsage> resourceUsageHistoricalData, List<ResourceUsage> resourceUsageTodaysData, RateCard rateCard)
        {
            CostStrategyContext costStrategy = new CostStrategyContext(resourceUsageHistoricalData, resourceUsageTodaysData, rateCard);
            return costStrategy.CalculateCosts();
        }

        private static List<ChartData> CalculateCostsForChart(List<ResourceUsage> resourceUsageHistoricalData, List<ResourceUsage> resourceUsageTodaysData, RateCard rateCard)
        {
            CostStrategyContext costStrategy = new CostStrategyContext(resourceUsageHistoricalData, resourceUsageTodaysData, rateCard);
            return costStrategy.CalculateCostsForChart();
        }

        [Route("api/resourceGroups")]
        public async Task<IHttpActionResult> Post(ResourceGroupResource resourceGroup)
        {
            var securityHelper = new SecurityHelper();
            var userHasAccess = await securityHelper.CheckUserPermissionToSubscription(resourceGroup.SubscriptionId);
            if (!userHasAccess)
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }

            var client = Utils.GetResourceManagementClient(resourceGroup.SubscriptionId);
            var rg = new ResourceGroup(resourceGroup.Location);
            var result = await client.ResourceGroups.CreateOrUpdateAsync(resourceGroup.ResourceGroupName, rg, new CancellationToken());

            await securityHelper.AddGroupsToAscContributorRole(resourceGroup.SubscriptionId, resourceGroup.ResourceGroupName, resourceGroup.ContributorGroups);

            return this.Ok(resourceGroup);
        }

        [Route("api/subscriptions/enrolled")]
        public async Task<IHttpActionResult> GetEnrolledSubscriptions()
        {
            var enrolledSubscriptions = this.coreRepository.GetEnrolledSubscriptionListByOrgId(ClaimsPrincipal.Current.TenantId());

            var currentUsersGroups = await Utils.GetCurrentUserGroups();

            var filteredSubscriptions = enrolledSubscriptions.Where(sub =>
            {
                if (sub.ContributorGroups != null)
                {
                    var contributorGroups = JArray.Parse(sub.ContributorGroups);
                    var ids = contributorGroups.Select(x => (string)x["id"]).ToList();
                    return currentUsersGroups.Intersect(ids).Count() > 0;
                }
                return false;
            });

            return Ok(filteredSubscriptions);
        }

        [Route("api/subscriptions/all-app-enrolled")]
        public IHttpActionResult GetAllAppEnrolledSubscriptions()
        {
            var enrolledSubscriptions = this.coreRepository.GetEnrolledSubscriptionListByOrgId(ClaimsPrincipal.Current.TenantId());
            //var currentUsersGroups = ClaimsPrincipal.Current.Claims.Where(x => x.Type == "groups").Select(x => x.Value).ToList();
            return Ok(enrolledSubscriptions);
        }

        [Route("api/subscriptions/enrolled")]
        public IHttpActionResult GetEnrolledSubscriptions(string subscriptionId)
        {
            var subscription = this.coreRepository.GetSubscription(subscriptionId);
            return Ok(subscription);
        }

        //private static ResourceGroupListParameters GetResourceGroupFilter()
        //{
        //    var filter = new ResourceGroupListParameters();
        //    filter.TagName = Utils.UserIdTagName;
        //    filter.TagValue = Utils.GetSignedInUserId();
        //    return filter;
        //}

    }
}
