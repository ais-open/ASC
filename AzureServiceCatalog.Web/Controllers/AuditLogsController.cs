using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using AzureServiceCatalog.Helpers;
using AzureServiceCatalog.Web.Models;
using Newtonsoft.Json.Linq;

namespace AzureServiceCatalog.Web.Controllers
{
    public class AuditLogsController : ApiController
    {
        public async Task<IHttpActionResult> GetByCorrelationId(string subscriptionId, string correlationId)
        {
            try
            {
                var json = await AzureResourceManagerUtil.GetAuditLogs(subscriptionId, correlationId);
                var responseMsg = this.Request.CreateResponse(HttpStatusCode.OK);
                responseMsg.Content = json.ToStringContent();
                IHttpActionResult response = ResponseMessage(responseMsg);
                return response;
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
