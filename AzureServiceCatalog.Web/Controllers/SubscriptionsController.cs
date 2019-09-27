using System;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using AzureServiceCatalog.Helpers;
using AzureServiceCatalog.Models;
using AzureServiceCatalog.Web.Models;
using System.Net;
using System.Net.Http;
using System.Web;

namespace AzureServiceCatalog.Web.Controllers
{
    [RoutePrefix("api/subscriptions")]
    public class SubscriptionsController : ApiController
    {
        private TableCoreRepository coreRepository = new TableCoreRepository();

        [Route("")]
        public async Task<IHttpActionResult> Get()
        {
            var thisOperationContext = new BaseOperationContext("SubscriptionsController:Get")
            {
                IpAddress = HttpContext.Current.Request.UserHostAddress,
                UserId = ClaimsPrincipal.Current.SignedInUserName(),
                UserName = ClaimsPrincipal.Current.Identity.Name
            };
            try
            {
                var tenantId = ClaimsPrincipal.Current.TenantId();
                var subscriptions = await AzureResourceManagerHelper.GetUserSubscriptions(tenantId, thisOperationContext);
                var dbSubscriptions = this.coreRepository.GetSubscriptionListByOrgId(tenantId, thisOperationContext);

                foreach (var subscription in subscriptions)
                {
                    var dbSub = dbSubscriptions.FirstOrDefault(x => x.Id == subscription.Id);
                    if (dbSub != null)
                    {
                        subscription.ConnectedOn = dbSub.ConnectedOn;
                        subscription.ConnectedBy = dbSub.ConnectedBy;
                        subscription.StorageName = dbSub.StorageName;
                        subscription.IsEnrolled = dbSub.IsEnrolled;
                        subscription.IsConnected = dbSub.IsConnected;
                        subscription.ContributorGroups = dbSub.ContributorGroups;
                    }
                }
                return this.Ok(subscriptions);
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

        [ADGroupAuthorize(SecurityGroupType.CanAdmin)]
        [Route("")]
        public async Task<IHttpActionResult> Post(SubscriptionsViewModel subscriptionsVM)
        {
            var thisOperationContext = new BaseOperationContext("SubscriptionsController:Post")
            {
                IpAddress = HttpContext.Current.Request.UserHostAddress,
                UserId = ClaimsPrincipal.Current.SignedInUserName(),
                UserName = ClaimsPrincipal.Current.Identity.Name
            };
            try
            {
                if (subscriptionsVM == null)
                {
                    ErrorInformation errorInformation = new ErrorInformation();
                    errorInformation.Code = "InvalidRequest";
                    errorInformation.Message = "Request body is invalid.";
                    return Content(HttpStatusCode.BadRequest, JObject.FromObject(errorInformation));
                } else
                {
                    var activationHelper = new ActivationHelper();
                    await activationHelper.SaveEnrolledSubscriptions(subscriptionsVM, thisOperationContext);
                    return this.Ok();
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
    }
}
