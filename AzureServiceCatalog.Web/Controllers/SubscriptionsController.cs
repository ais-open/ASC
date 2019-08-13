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

namespace AzureServiceCatalog.Web.Controllers
{
    [RoutePrefix("api/subscriptions")]
    public class SubscriptionsController : ApiController
    {
        private TableCoreRepository coreRepository = new TableCoreRepository();

        [Route("")]
        public async Task<IHttpActionResult> Get()
        {
            try
            {
                var tenantId = ClaimsPrincipal.Current.TenantId();
                var subscriptions = await AzureResourceManagerUtil.GetUserSubscriptions(tenantId);
                var dbSubscriptions = this.coreRepository.GetSubscriptionListByOrgId(tenantId);

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
            catch (Exception)
            {
                return Content(HttpStatusCode.InternalServerError, JObject.FromObject(ErrorInformation.GetInternalServerErrorInformation()));
            }
            finally
            {

            }
        }

        [ADGroupAuthorize(SecurityGroupType.CanAdmin)]
        [Route("")]
        public async Task<IHttpActionResult> Post(SubscriptionsViewModel subscriptionsVM)
        {
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
                    await activationHelper.SaveEnrolledSubscriptions(subscriptionsVM);
                    return this.Ok();
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
    }
}
