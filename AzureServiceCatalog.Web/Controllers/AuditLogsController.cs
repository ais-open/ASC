using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using AzureServiceCatalog.Helpers;
using AzureServiceCatalog.Models;
using AzureServiceCatalog.Web.Models;
using Newtonsoft.Json.Linq;

namespace AzureServiceCatalog.Web.Controllers
{
    public class AuditLogsController : ApiController
    {
        public async Task<IHttpActionResult> GetByCorrelationId(string subscriptionId, string correlationId)
        {
            var thisOperationContext = new BaseOperationContext("AuditLogsController:GetByCorrelationId");
            thisOperationContext.IpAddress = HttpContext.Current.Request.UserHostAddress;
            thisOperationContext.UserId = ClaimsPrincipal.Current.SignedInUserName();
            thisOperationContext.UserName = ClaimsPrincipal.Current.Identity.Name;
            try
            {
                var json = await AzureResourceManagerHelper.GetAuditLogs(subscriptionId, correlationId, thisOperationContext);
                var responseMsg = this.Request.CreateResponse(HttpStatusCode.OK);
                responseMsg.Content = json.ToStringContent();
                IHttpActionResult response = ResponseMessage(responseMsg);
                return response;
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
