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
using AzureServiceCatalog.Models;
using AzureServiceCatalog.Helpers;
using AzureServiceCatalog.Web.Models;
using System.Text;

namespace AzureServiceCatalog.Web.Controllers
{
    public class ResourcesController : ApiController
    {
        TableCoreRepository coreRepository = new TableCoreRepository();
        BillingHelper billingHelper = new BillingHelper();

        [Route("api/subscriptions/{subscriptionId}/resourceGroups")]
        public async Task<IHttpActionResult> GetResourceGroupsBySubscription(string subscriptionId)
        {
            try
            {
                var json = await AzureResourceManagerUtil.GetUserResourceGroups(subscriptionId);
                var responseMsg = this.Request.CreateResponse(HttpStatusCode.OK);
                responseMsg.Content = json.ToStringContent();
                IHttpActionResult response = ResponseMessage(responseMsg);
                return response;
            }
            catch (Exception)
            {
                return Content(HttpStatusCode.InternalServerError, JObject.FromObject(ErrorInformation.GetInternalServerErrorInformation()));
            }
            finally
            {

            }
        }

        [Route("api/subscriptions/{subscriptionId}/resourceGroupsAll")]
        public async Task<IHttpActionResult> GetAllResourceGroupsBySubscription(string subscriptionId)
        {
            try
            {
                var client = Utils.GetResourceManagementClient(subscriptionId);
                var list = await client.ResourceGroups.ListAsync(new ResourceGroupListParameters());
                return this.Ok(list);
            }
            catch (Exception)
            {
                return Content(HttpStatusCode.InternalServerError, JObject.FromObject(ErrorInformation.GetInternalServerErrorInformation()));
            }
            finally
            {

            }
        }

        [Route("api/subscriptions/{subscriptionId}/user-resource-groups")]
        public async Task<IHttpActionResult> GetAllUserResourceGroupsBySubscription(string subscriptionId)
        {
            try
            {
                var json = await AzureResourceManagerUtil.GetUserResourceGroups(subscriptionId);
                var responseMsg = this.Request.CreateResponse(HttpStatusCode.OK);
                responseMsg.Content = json.ToStringContent();
                IHttpActionResult response = ResponseMessage(responseMsg);
                return response;
            }
            catch (Exception)
            {
                return Content(HttpStatusCode.InternalServerError, JObject.FromObject(ErrorInformation.GetInternalServerErrorInformation()));
            }
            finally
            {

            }
        }

        [Route("api/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}")]
        public async Task<IHttpActionResult> GetResourceGroupResources(string resourceGroupName, string subscriptionId)
        {
            try
            {
                var client = Utils.GetResourceManagementClient(subscriptionId);
                var resourceList = await client.Resources.ListAsync(new ResourceListParameters { ResourceGroupName = resourceGroupName });
                List<ResourceUsage> resourceUsageData = await billingHelper.GetUsage(resourceList, subscriptionId);
                return this.Ok(resourceUsageData);
            }
            catch (Exception)
            {
                return Content(HttpStatusCode.InternalServerError, JObject.FromObject(ErrorInformation.GetInternalServerErrorInformation()));
            }
            finally
            {

            }
        }

        [Route("api/subscriptions/{subscriptionId}/chartData/{resourceGroupName}")]
        public async Task<IHttpActionResult> GetChartData(string resourceGroupName, string subscriptionId)
        {
            try
            {
                var client = Utils.GetResourceManagementClient(subscriptionId);
                var resourceList = await client.Resources.ListAsync(new ResourceListParameters { ResourceGroupName = resourceGroupName });
                var chartData = await billingHelper.GetChartData(resourceList, subscriptionId);
                return this.Ok(chartData);
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
                return Content(HttpStatusCode.InternalServerError, JObject.FromObject(ErrorInformation.GetInternalServerErrorInformation()));
            }
            finally
            {

            }
        }

        [Route("api/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/full")]
        public async Task<IHttpActionResult> GetResourceGroupData(string resourceGroupName, string subscriptionId)
        {
            try
            {
                var client = Utils.GetResourceManagementClient(subscriptionId);
                var resourceList = await client.Resources.ListAsync(new ResourceListParameters { ResourceGroupName = resourceGroupName });

                var resourceUsageData = await billingHelper.GetUsage(resourceList, subscriptionId);
                var chartData = await billingHelper.GetChartData(resourceList, subscriptionId);

                var rgData = new ResourceGroupData
                {
                    ResourceGroupName = resourceGroupName,
                    ResourcesUsages = resourceUsageData,
                    ChartData = chartData
                };
                return this.Ok(rgData);
            }
            catch (Exception)
            {
                return Content(HttpStatusCode.InternalServerError, JObject.FromObject(ErrorInformation.GetInternalServerErrorInformation()));
            }
            finally
            {

            }
        }

        [Route("api/resourceGroups")]
        public async Task<IHttpActionResult> Post(ResourceGroupResource resourceGroup)
        {
            try
            { 
                if (resourceGroup == null)
                {
                    ErrorInformation errorInformation = new ErrorInformation();
                    errorInformation.Code = "InvalidRequest";
                    errorInformation.Message = "Request body is invalid.";
                    return Content(HttpStatusCode.BadRequest, JObject.FromObject(errorInformation));
                } else
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
            }
            catch (Exception)
            {
                return Content(HttpStatusCode.InternalServerError, JObject.FromObject(ErrorInformation.GetInternalServerErrorInformation()));
            }
            finally
            {

            }
        }

        [Route("api/subscriptions/enrolled")]
        public async Task<IHttpActionResult> GetEnrolledSubscriptions()
        {
            try
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
            catch (Exception)
            {
                return Content(HttpStatusCode.InternalServerError, JObject.FromObject(ErrorInformation.GetInternalServerErrorInformation()));
            }
            finally
            {

            }
        }

        [Route("api/subscriptions/all-app-enrolled")]
        public IHttpActionResult GetAllAppEnrolledSubscriptions()
        {
            try
            {
                var enrolledSubscriptions = this.coreRepository.GetEnrolledSubscriptionListByOrgId(ClaimsPrincipal.Current.TenantId());
                //var currentUsersGroups = ClaimsPrincipal.Current.Claims.Where(x => x.Type == "groups").Select(x => x.Value).ToList();
                return Ok(enrolledSubscriptions);
            }
            catch (Exception)
            {
                return Content(HttpStatusCode.InternalServerError, JObject.FromObject(ErrorInformation.GetInternalServerErrorInformation()));
            }
            finally
            {

            }
        }

        [Route("api/subscriptions/enrolled")]
        public IHttpActionResult GetEnrolledSubscriptions(string subscriptionId)
        {
            try
            {
                var subscription = this.coreRepository.GetSubscription(subscriptionId);
                return Ok(subscription);
            }
            catch (Exception)
            {
                return Content(HttpStatusCode.InternalServerError, JObject.FromObject(ErrorInformation.GetInternalServerErrorInformation()));
            }
            finally
            {

            }
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
