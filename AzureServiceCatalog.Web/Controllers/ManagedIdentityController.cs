using AzureServiceCatalog.Web.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using AzureServiceCatalog.Helpers;
using AzureServiceCatalog.Models;
using System.Web;
using System.Security.Claims;
using System;

namespace AzureServiceCatalog.Web.Controllers
{
    [RoutePrefix("api/managed-identities")]
    public class ManagedIdentityController : ApiController
    {
        private ManagedIdentityHelper client = new ManagedIdentityHelper();

        [Route("userAssignedIdentities")]
        public async Task<IHttpActionResult> Get(string subscriptionId)
        {
            var thisOperationContext = new BaseOperationContext("ManagedIdentityController:Get")
            {
                IpAddress = HttpContext.Current.Request.UserHostAddress,
                UserId = ClaimsPrincipal.Current.SignedInUserName(),
                UserName = ClaimsPrincipal.Current.Identity.Name
            };
            try
            {
                var userAssignedIdentities = await this.client.GetUserAssignedIdentities(subscriptionId, thisOperationContext);
                var list = new List<object>();
                dynamic updatedIdentities= JObject.Parse(userAssignedIdentities);
                foreach (var item in updatedIdentities.value)
                {
                    var managedIdentityItem = new ManagedIdentity
                    {
                        Id = item.id,
                        Name = item.name,
                        Location = item.location,
                        Type = item.type,
                        ClientId = item.properties.clientId,
                        PrincipalId = item.properties.principalId,
                        TenantId = item.properties.tenantId,
                        Tags = item.tags
                    };
                    list.Add(managedIdentityItem);
                }
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
    }
}