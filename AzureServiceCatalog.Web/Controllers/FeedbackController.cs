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
    public class FeedbackController : ApiController
    {
        NotificationHelper notificationHelper = new NotificationHelper();
        // POST: api/Feedback
        public async Task<IHttpActionResult> PostAsync([FromBody]FeedbackViewModel model)
        {
            var thisOperationContext = new BaseOperationContext("FeedbackController:Post")
            {
                IpAddress = HttpContext.Current.Request.UserHostAddress
            };
            try
            {
                ErrorInformation errorInformation = null;
                if (model == null)
                {
                    errorInformation = new ErrorInformation();
                    errorInformation.Code = "InvalidRequest";
                    errorInformation.Message = "Request body is invalid.";
                    //return new ActionResults.JsonActionResult(HttpStatusCode.BadRequest, JObject.FromObject(errorInformation));
                    return Content(HttpStatusCode.BadRequest, JObject.FromObject(errorInformation));
                } else
                {
                    await notificationHelper.SendFeedbackNotificationAsync(model, thisOperationContext);
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

        public IHttpActionResult Get()
        {
            var thisOperationContext = new BaseOperationContext("FeedbackController:Get")
            {
                IpAddress = HttpContext.Current.Request.UserHostAddress
                //UserId = ClaimsPrincipal.Current.SignedInUserName(),
                //UserName = ClaimsPrincipal.Current.Identity.Name
            };
            try
            {
                var fbViewModel = new FeedbackViewModel();
                fbViewModel.Name = ClaimsPrincipal.Current.Name();
                fbViewModel.Email = ClaimsPrincipal.Current.Upn();
                return Ok(fbViewModel);
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
