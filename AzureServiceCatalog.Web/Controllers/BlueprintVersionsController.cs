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
    [RoutePrefix("api/blueprint-versions")]
    public class BlueprintVersionsController : ApiController
    {
        private BlueprintsHelper client = new BlueprintsHelper();

        [Route("{blueprintName}")]
        public async Task<IHttpActionResult> Get(string subscriptionId, string blueprintName)
        {
            var thisOperationContext = new BaseOperationContext("BlueprintVersionsController:Get")
            {
                IpAddress = HttpContext.Current.Request.UserHostAddress,
                UserId = ClaimsPrincipal.Current.SignedInUserName(),
                UserName = ClaimsPrincipal.Current.Identity.Name
            };
            try
            {
                var blueprintVersions = await this.client.GetBlueprintVersions(subscriptionId, blueprintName, thisOperationContext);
                var list = new List<object>();
                dynamic updatedBlueprintVersions = JObject.Parse(blueprintVersions);
                foreach (var item in updatedBlueprintVersions.value)
                {
                    var blueprintVersionItem = new BlueprintVersion
                    {
                        Id = item.id,
                        Name = item.name,
                        BlueprintName = item.properties.blueprintName,
                        Type = item.type,
                        Scope = item.properties.targetScope,
                        Description = item.properties.description,
                        CreatedDate = item.properties.status.timeCreated,
                        LastModifiedDate = item.properties.status.lastModified,
                        ResourceGroups = item.properties.resourceGroups,
                        Parameters = item.properties.parameters
                    };
                    list.Add(blueprintVersionItem);
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