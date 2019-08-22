using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using AzureServiceCatalog.Helpers;
using AzureServiceCatalog.Models;
using AzureServiceCatalog.Web.Models;
using Newtonsoft.Json.Linq;

namespace AzureServiceCatalog.Web.Controllers
{
    [RoutePrefix("api/providers")]
    public class ProvidersController : ApiController
    {
        [Route("storage")]
        public async Task<IHttpActionResult> GetStorageProvider(string subscriptionId)
        {
            var thisOperationContext = new BaseOperationContext("ProvidersController:GetStorageProvider");
            thisOperationContext.IpAddress = HttpContext.Current.Request.UserHostAddress;
            thisOperationContext.UserId = ClaimsPrincipal.Current.SignedInUserName();
            thisOperationContext.UserName = ClaimsPrincipal.Current.Identity.Name;
            try
            {
                var json = await AzureResourceManagerHelper.GetStorageProvider(subscriptionId, thisOperationContext);
                var responseMsg = this.Request.CreateResponse(HttpStatusCode.OK);
                responseMsg.Content = new StringContent(json, Encoding.UTF8, "application/json");
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

        [Route("resources")]
        public async Task<IHttpActionResult> GetResourcesProvider(string subscriptionId)
        {
            var thisOperationContext = new BaseOperationContext("ProvidersController:GetResourcesProvider");
            thisOperationContext.IpAddress = HttpContext.Current.Request.UserHostAddress;
            thisOperationContext.UserId = ClaimsPrincipal.Current.SignedInUserName();
            thisOperationContext.UserName = ClaimsPrincipal.Current.Identity.Name;
            try
            {
                var json = await AzureResourceManagerHelper.GetResourcesProvider(subscriptionId, thisOperationContext);
                var responseMsg = this.Request.CreateResponse(HttpStatusCode.OK);
                responseMsg.Content = new StringContent(json, Encoding.UTF8, "application/json");
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
