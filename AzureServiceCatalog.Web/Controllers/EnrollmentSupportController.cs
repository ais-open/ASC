using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using AzureServiceCatalog.Helpers;
using AzureServiceCatalog.Models;
using AzureServiceCatalog.Web.Models;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace AzureServiceCatalog.Web.Controllers
{
    public class EnrollmentSupportController : ApiController
    {
        NotificationHelper notificationHelper = new NotificationHelper();

        [AllowAnonymous]
        public async Task<IHttpActionResult> PostAsync([FromBody]EnrollmentSupportViewModel model)
        {
            var thisOperationContext = new BaseOperationContext("EnrollmentSupportController:Post")
            {
                IpAddress = HttpContext.Current.Request.UserHostAddress
            };
            try
            {
                if (model == null)
                {
                    ErrorInformation errorInformation = new ErrorInformation();
                    errorInformation.Code = "InvalidRequest";
                    errorInformation.Message = "Request body is invalid.";
                    return Content(HttpStatusCode.BadRequest, JObject.FromObject(errorInformation));
                } else
                {
                    await notificationHelper.SendSupportNotificationAsync(model, thisOperationContext);
                    return Ok();
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
