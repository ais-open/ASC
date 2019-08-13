using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
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
        public IHttpActionResult Post([FromBody]FeedbackViewModel model)
        {
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
                    notificationHelper.SendFeedbackNotification(model);
                    return Ok();
                }
            }
            catch (Exception)
            {
                return Content(HttpStatusCode.InternalServerError, JObject.FromObject(ErrorInformation.GetInternalServerErrorInformation()));
            }
            finally
            {

            }
        }

        public IHttpActionResult Get()
        {
            try
            {
                var fbViewModel = new FeedbackViewModel();
                fbViewModel.Name = ClaimsPrincipal.Current.Name();
                fbViewModel.Email = ClaimsPrincipal.Current.Upn();
                return Ok(fbViewModel);
            }
            catch (Exception)
            {
               return Content(HttpStatusCode.InternalServerError, JObject.FromObject(ErrorInformation.GetInternalServerErrorInformation()));
            }
            finally
            {

            }
        }
    }
}
