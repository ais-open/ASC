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
    [RoutePrefix("api/blueprint-assignment-operations")]
    public class BlueprintAssignmentOperationsController : ApiController
    {
        private BlueprintsHelper client = new BlueprintsHelper();

        [Route("{blueprintAssignmentName}")]
        public async Task<IHttpActionResult> Get(string subscriptionId, string blueprintAssignmentName)
        {
            var thisOperationContext = new BaseOperationContext("BlueprintAssignmentOperationsController:Get")
            {
                IpAddress = HttpContext.Current.Request.UserHostAddress,
                UserId = ClaimsPrincipal.Current.SignedInUserName(),
                UserName = ClaimsPrincipal.Current.Identity.Name
            };
            try
            {
                var assignmentOperations = await this.client.GetBlueprintAssignmentOperations(subscriptionId, blueprintAssignmentName,thisOperationContext);
                var list = new List<object>();
                dynamic updatedAssignmentOperations = JObject.Parse(assignmentOperations);
                return this.Ok(updatedAssignmentOperations);
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