using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AzureServiceCatalog.Helpers;
using AzureServiceCatalog.Models;

namespace AzureServiceCatalog.Web.Controllers
{
    public class EnrollmentSupportController : ApiController
    {
        NotificationHelper notificationHelper = new NotificationHelper();

        [AllowAnonymous]
        public IHttpActionResult Post([FromBody]EnrollmentSupportViewModel model)
        {
            notificationHelper.SendSupportNotification(model);
            return Ok();
        }
    }
}
