using AzureServiceCatalog.Web.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AzureServiceCatalog.Web.Controllers
{
    public class EnrollmentSupportController : ApiController
    {
        NotificationProcessor notificationProcessor = new NotificationProcessor();

        [AllowAnonymous]
        public IHttpActionResult Post([FromBody]EnrollmentSupportViewModel model)
        {
            notificationProcessor.SendSupportNotification(model);
            return Ok();
        }
    }
}
