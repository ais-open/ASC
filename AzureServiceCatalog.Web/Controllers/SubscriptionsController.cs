using AzureServiceCatalog.Web.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace AzureServiceCatalog.Web.Controllers
{
    [RoutePrefix("api/subscriptions")]
    public class SubscriptionsController : ApiController
    {
        private TableCoreRepository coreRepository = new TableCoreRepository();

        [Route("")]
        public List<Subscription> Get()
        {
            var tenantId = ClaimsPrincipal.Current.TenantId();
            var subscriptions = AzureResourceManagerUtil.GetUserSubscriptions(tenantId);
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
            return subscriptions;
        }

        [ADGroupAuthorize(SecurityGroupType.CanAdmin)]
        [Route("")]
        public async Task<IHttpActionResult> Post(SubscriptionsViewModel subscriptionsVM)
        {
            var activationProcessor = new ActivationProcessor();
            await activationProcessor.SaveEnrolledSubscriptions(subscriptionsVM);
            return this.Ok();
        }
    }
}
