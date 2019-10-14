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
using System.Web;

namespace AzureServiceCatalog.Web.Controllers
{
    public class ResourcesController : ApiController
    {
        TableCoreRepository coreRepository = new TableCoreRepository();
        BillingHelper billingHelper = new BillingHelper();
        ConsumptionHelper consumptionHelper = new ConsumptionHelper();

        [Route("api/subscriptions/{subscriptionId}/resourceGroups")]
        public async Task<IHttpActionResult> GetResourceGroupsBySubscription(string subscriptionId)
        {
            var thisOperationContext = new BaseOperationContext("ResourcesController:GetResourceGroupsBySubscription")
            {
                IpAddress = HttpContext.Current.Request.UserHostAddress,
                UserId = ClaimsPrincipal.Current.SignedInUserName(),
                UserName = ClaimsPrincipal.Current.Identity.Name
            };
            try
            {
                var json = await AzureResourceManagerHelper.GetUserResourceGroups(subscriptionId, thisOperationContext);
                var responseMsg = this.Request.CreateResponse(HttpStatusCode.OK);
                responseMsg.Content = json.ToStringContent();
                IHttpActionResult response = ResponseMessage(responseMsg);
                return response;
            }
            catch (Exception ex)
            {
                TraceHelper.TraceError(thisOperationContext.OperationId, thisOperationContext.OperationName, ex);
                return Content(HttpStatusCode.InternalServerError, JObject.FromObject(ErrorInformation.GetInternalServerErrorInformation(thisOperationContext.OperationId, thisOperationContext.Timestamp)));
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        [Route("api/subscriptions/{subscriptionId}/resourceGroupsAll")]
        public async Task<IHttpActionResult> GetAllResourceGroupsBySubscription(string subscriptionId)
        {
            var thisOperationContext = new BaseOperationContext("ResourcesController:GetAllResourceGroupsBySubscription")
            {
                IpAddress = HttpContext.Current.Request.UserHostAddress,
                UserId = ClaimsPrincipal.Current.SignedInUserName(),
                UserName = ClaimsPrincipal.Current.Identity.Name
            };
            try
            {
                var client = Helpers.Helpers.GetResourceManagementClient(subscriptionId, thisOperationContext);
                var list = await client.ResourceGroups.ListAsync(new ResourceGroupListParameters());
                return this.Ok(list);
            }
            catch (Exception ex)
            {
                TraceHelper.TraceError(thisOperationContext.OperationId, thisOperationContext.OperationName, ex);
                return Content(HttpStatusCode.InternalServerError, JObject.FromObject(ErrorInformation.GetInternalServerErrorInformation(thisOperationContext.OperationId, thisOperationContext.Timestamp)));
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        [Route("api/subscriptions/{subscriptionId}/user-resource-groups")]
        public async Task<IHttpActionResult> GetAllUserResourceGroupsBySubscription(string subscriptionId)
        {
            var thisOperationContext = new BaseOperationContext("ResourcesController:GetAllUserResourceGroupsBySubscription")
            {
                IpAddress = HttpContext.Current.Request.UserHostAddress,
                UserId = ClaimsPrincipal.Current.SignedInUserName(),
                UserName = ClaimsPrincipal.Current.Identity.Name
            };
            try
            {
                var json = await AzureResourceManagerHelper.GetUserResourceGroups(subscriptionId, thisOperationContext);
                var responseMsg = this.Request.CreateResponse(HttpStatusCode.OK);
                responseMsg.Content = json.ToStringContent();
                IHttpActionResult response = ResponseMessage(responseMsg);
                return response;
            }
            catch (Exception ex)
            {
                TraceHelper.TraceError(thisOperationContext.OperationId, thisOperationContext.OperationName, ex);
                return Content(HttpStatusCode.InternalServerError, JObject.FromObject(ErrorInformation.GetInternalServerErrorInformation(thisOperationContext.OperationId, thisOperationContext.Timestamp)));
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        [Route("api/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}")]
        public async Task<IHttpActionResult> GetResourceGroupResources(string resourceGroupName, string subscriptionId)
        {
            var thisOperationContext = new BaseOperationContext("ResourcesController:GetResourceGroupResources")
            {
                IpAddress = HttpContext.Current.Request.UserHostAddress,
                UserId = ClaimsPrincipal.Current.SignedInUserName(),
                UserName = ClaimsPrincipal.Current.Identity.Name
            };
            try
            {
                //Consumption API
                //List<ResourceUsageDetails> resourcesUsageDetailsData = await consumptionHelper.GetUsage(resourceGroupName, subscriptionId, thisOperationContext);

                var client = Helpers.Helpers.GetResourceManagementClient(subscriptionId, thisOperationContext);
                var resourceList = await client.Resources.ListAsync(new ResourceListParameters { ResourceGroupName = resourceGroupName });
                List<ResourceUsage> resourceUsageData = await billingHelper.GetUsage(resourceList, subscriptionId, thisOperationContext);
                return this.Ok(resourceUsageData);
            }
            catch (Exception ex)
            {
                TraceHelper.TraceError(thisOperationContext.OperationId, thisOperationContext.OperationName, ex);
                return Content(HttpStatusCode.InternalServerError, JObject.FromObject(ErrorInformation.GetInternalServerErrorInformation(thisOperationContext.OperationId, thisOperationContext.Timestamp)));
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        [Route("api/subscriptions/{subscriptionId}/chartData/{resourceGroupName}")]
        public async Task<IHttpActionResult> GetChartData(string resourceGroupName, string subscriptionId)
        {
            var thisOperationContext = new BaseOperationContext("ResourcesController:GetChartData")
            {
                IpAddress = HttpContext.Current.Request.UserHostAddress,
                UserId = ClaimsPrincipal.Current.SignedInUserName(),
                UserName = ClaimsPrincipal.Current.Identity.Name
            };
            try
            {
                //Consumption API
                //List<ResourceUsageDetails> resourcesUsageDetailsData = await consumptionHelper.GetUsage(resourceGroupName, subscriptionId, thisOperationContext);

                var client = Helpers.Helpers.GetResourceManagementClient(subscriptionId, thisOperationContext);
                var resourceList = await client.Resources.ListAsync(new ResourceListParameters { ResourceGroupName = resourceGroupName });
                var chartData = await billingHelper.GetChartData(resourceList, subscriptionId, thisOperationContext);
                return this.Ok(chartData);
            }
            catch (Exception ex)
            {
                TraceHelper.TraceError(thisOperationContext.OperationId, thisOperationContext.OperationName, ex);
                return Content(HttpStatusCode.InternalServerError, JObject.FromObject(ErrorInformation.GetInternalServerErrorInformation(thisOperationContext.OperationId, thisOperationContext.Timestamp)));
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        [Route("api/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/full")]
        public async Task<IHttpActionResult> GetResourceGroupData(string resourceGroupName, string subscriptionId)
        {
            var thisOperationContext = new BaseOperationContext("ResourcesController:GetResourceGroupData")
            {
                IpAddress = HttpContext.Current.Request.UserHostAddress,
                UserId = ClaimsPrincipal.Current.SignedInUserName(),
                UserName = ClaimsPrincipal.Current.Identity.Name
            };
            try
            {
                var client = Helpers.Helpers.GetResourceManagementClient(subscriptionId, thisOperationContext);
                var resourceList = await client.Resources.ListAsync(new ResourceListParameters { ResourceGroupName = resourceGroupName });

                //consumptionAPI
                List<ResourceUsageDetails> resourceUsageMonthData = await consumptionHelper.GetConsumptionUsageDetailsForLast30Days(resourceList, subscriptionId, thisOperationContext);
                List<ResourceUsageDetails> resourceUsageTodayData = await consumptionHelper.GetConsumptionUsageDetailsForToday(resourceList, subscriptionId, thisOperationContext);
                var resourcesUsageDetailsData = consumptionHelper.GetUsage(resourceUsageMonthData, resourceUsageTodayData, thisOperationContext);
                var consumptionChartData = consumptionHelper.GetChartData(resourceUsageMonthData, resourceUsageTodayData, thisOperationContext);

                //var resourceUsageData = await billingHelper.GetUsage(resourceList, subscriptionId, thisOperationContext);
                //var chartData = await billingHelper.GetChartData(resourceList, subscriptionId, thisOperationContext);

                var rgData = new ResourceGroupData
                {
                    ResourceGroupName = resourceGroupName,
                    ResourcesUsages = resourcesUsageDetailsData,
                    ChartData = consumptionChartData
                };
                return this.Ok(rgData);
            }
            catch (Exception ex)
            {
                TraceHelper.TraceError(thisOperationContext.OperationId, thisOperationContext.OperationName, ex);
                return Content(HttpStatusCode.InternalServerError, JObject.FromObject(ErrorInformation.GetInternalServerErrorInformation(thisOperationContext.OperationId, thisOperationContext.Timestamp)));
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        [Route("api/resourceGroups")]
        public async Task<IHttpActionResult> Post(ResourceGroupResource resourceGroup)
        {
            var thisOperationContext = new BaseOperationContext("ResourcesController:Post")
            {
                IpAddress = HttpContext.Current.Request.UserHostAddress,
                UserId = ClaimsPrincipal.Current.SignedInUserName(),
                UserName = ClaimsPrincipal.Current.Identity.Name
            };
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
                    var userHasAccess = await securityHelper.CheckUserPermissionToSubscription(resourceGroup.SubscriptionId, thisOperationContext);
                    if (!userHasAccess)
                    {
                        throw new HttpResponseException(HttpStatusCode.Forbidden);
                    }

                    var client = Helpers.Helpers.GetResourceManagementClient(resourceGroup.SubscriptionId, thisOperationContext);
                    var rg = new ResourceGroup(resourceGroup.Location);
                    var result = await client.ResourceGroups.CreateOrUpdateAsync(resourceGroup.ResourceGroupName, rg, new CancellationToken());

                    await securityHelper.AddGroupsToAscContributorRole(resourceGroup.SubscriptionId, resourceGroup.ResourceGroupName, resourceGroup.ContributorGroups, thisOperationContext);

                    return this.Ok(resourceGroup);
                }
            }
            catch (Exception ex)
            {
                TraceHelper.TraceError(thisOperationContext.OperationId, thisOperationContext.OperationName, ex);
                return Content(HttpStatusCode.InternalServerError, JObject.FromObject(ErrorInformation.GetInternalServerErrorInformation(thisOperationContext.OperationId, thisOperationContext.Timestamp)));
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        [Route("api/subscriptions/enrolled")]
        public async Task<IHttpActionResult> GetEnrolledSubscriptions()
        {
            var thisOperationContext = new BaseOperationContext("ResourcesController:GetEnrolledSubscriptions")
            {
                IpAddress = HttpContext.Current.Request.UserHostAddress,
                UserId = ClaimsPrincipal.Current.SignedInUserName(),
                UserName = ClaimsPrincipal.Current.Identity.Name
            };
            try
            {
                var enrolledSubscriptions = this.coreRepository.GetEnrolledSubscriptionListByOrgId(ClaimsPrincipal.Current.TenantId(), thisOperationContext);
                var currentUsersGroups = await Helpers.Helpers.GetCurrentUserGroups(thisOperationContext);
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
            catch (Exception ex)
            {
                TraceHelper.TraceError(thisOperationContext.OperationId, thisOperationContext.OperationName, ex);
                return Content(HttpStatusCode.InternalServerError, JObject.FromObject(ErrorInformation.GetInternalServerErrorInformation(thisOperationContext.OperationId, thisOperationContext.Timestamp)));
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        [Route("api/subscriptions/all-app-enrolled")]
        public IHttpActionResult GetAllAppEnrolledSubscriptions()
        {
            var thisOperationContext = new BaseOperationContext("ResourcesController:GetAllAppEnrolledSubscriptions")
            {
                IpAddress = HttpContext.Current.Request.UserHostAddress,
                UserId = ClaimsPrincipal.Current.SignedInUserName(),
                UserName = ClaimsPrincipal.Current.Identity.Name
            };
            try
            {
                var enrolledSubscriptions = this.coreRepository.GetEnrolledSubscriptionListByOrgId(ClaimsPrincipal.Current.TenantId(), thisOperationContext);
                //var currentUsersGroups = ClaimsPrincipal.Current.Claims.Where(x => x.Type == "groups").Select(x => x.Value).ToList();
                return Ok(enrolledSubscriptions);
            }
            catch (Exception ex)
            {
                TraceHelper.TraceError(thisOperationContext.OperationId, thisOperationContext.OperationName, ex);
                return Content(HttpStatusCode.InternalServerError, JObject.FromObject(ErrorInformation.GetInternalServerErrorInformation(thisOperationContext.OperationId, thisOperationContext.Timestamp)));
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        [Route("api/subscriptions/enrolled")]
        public IHttpActionResult GetEnrolledSubscriptions(string subscriptionId)
        {
            var thisOperationContext = new BaseOperationContext("ResourcesController:GetEnrolledSubscriptions")
            {
                IpAddress = HttpContext.Current.Request.UserHostAddress,
                UserId = ClaimsPrincipal.Current.SignedInUserName(),
                UserName = ClaimsPrincipal.Current.Identity.Name
            };
            try
            {
                var subscription = this.coreRepository.GetSubscription(subscriptionId, thisOperationContext);
                return Ok(subscription);
            }
            catch (Exception ex)
            {
                TraceHelper.TraceError(thisOperationContext.OperationId, thisOperationContext.OperationName, ex);
                return Content(HttpStatusCode.InternalServerError, JObject.FromObject(ErrorInformation.GetInternalServerErrorInformation(thisOperationContext.OperationId, thisOperationContext.Timestamp)));
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
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
