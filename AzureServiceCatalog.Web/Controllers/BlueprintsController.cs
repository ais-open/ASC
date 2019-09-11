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
    [RoutePrefix("api/blueprints")]
    public class BlueprintsController : ApiController
    {
        private BlueprintsHelper client = new BlueprintsHelper();

        [Route("")]
        public async Task<IHttpActionResult> Get(string subscriptionId)
        {
            var thisOperationContext = new BaseOperationContext("BlueprintsController:Get")
            {
                IpAddress = HttpContext.Current.Request.UserHostAddress,
                UserId = ClaimsPrincipal.Current.SignedInUserName(),
                UserName = ClaimsPrincipal.Current.Identity.Name
            };
            try
            {
                var blueprints = await this.client.GetBlueprintDefinitions(subscriptionId, thisOperationContext);
                var list = new List<object>();
                dynamic updatedBlueprints = JObject.Parse(blueprints);
                foreach (var item in updatedBlueprints.value)
                {
                    var blueprintItem = new Blueprint
                    {
                        Id = item.id,
                        Name = item.name,
                        Type = item.type,
                        Scope = item.properties.targetScope,
                        Description = item.properties.description,
                        CreatedDate = item.properties.status.timeCreated,
                        LastModifiedDate = item.properties.status.lastModified,
                        Properties = item.properties,
                    };
                    list.Add(blueprintItem);
                }
                return this.Ok(list);
            }
            catch (Exception ex)
            {
                TraceHelper.TraceError(thisOperationContext.OperationId, thisOperationContext.OperationName, ex);
                return Content(HttpStatusCode.InternalServerError, JObject.FromObject(ErrorInformation.GetInternalServerErrorInformation()));
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }
    }
}