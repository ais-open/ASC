using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using AzureServiceCatalog.Helpers;

namespace AzureServiceCatalog.Web.Controllers
{
    public class AuditLogsController : ApiController
    {
        public async Task<HttpResponseMessage> GetByCorrelationId(string subscriptionId, string correlationId)
        {
            var json = await AzureResourceManagerUtil.GetAuditLogs(subscriptionId, correlationId);
            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = json.ToStringContent();
            return response;
        }
    }
}
