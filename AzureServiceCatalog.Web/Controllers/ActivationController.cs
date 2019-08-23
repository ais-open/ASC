using System;
using System.Threading.Tasks;
using System.Web.Http;
using AzureServiceCatalog.Models;
using AzureServiceCatalog.Helpers;
using AzureServiceCatalog.Web.Models;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Web;
using System.Security.Claims;

namespace AzureServiceCatalog.Web.Controllers
{
    public class ActivationController : ApiController
    {
        public async Task<IHttpActionResult> Post(ActivationInfo activationInfo)
        {
            var thisOperationContext = new BaseOperationContext("ActivationController:Post")
            {
                IpAddress = HttpContext.Current.Request.UserHostAddress,
                UserId = ClaimsPrincipal.Current.SignedInUserName(),
                UserName = ClaimsPrincipal.Current.Identity.Name
            };
            try
            {
                if (activationInfo == null)
                {
                    ErrorInformation errorInformation = new ErrorInformation();
                    errorInformation.Code = "InvalidRequest";
                    errorInformation.Message = "Request body is invalid.";
                    return Content(HttpStatusCode.BadRequest, JObject.FromObject(errorInformation));
                } else
                {
                    var activationHelper = new ActivationHelper();
                    await activationHelper.SaveActivation(activationInfo, thisOperationContext);
                    return this.Ok();
                }
            }
            catch (UnauthorizedAccessException authEx)
            {
                TraceHelper.TraceError(thisOperationContext.OperationId, thisOperationContext.OperationName, authEx);
                return Content(HttpStatusCode.InternalServerError, JObject.FromObject(authEx));

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
