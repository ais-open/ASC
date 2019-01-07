using AzureServiceCatalog.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace AzureServiceCatalog.Web.Controllers
{
    [RoutePrefix("api/providers")]
    public class ProvidersController : ApiController
    {
        [Route("storage")]
        public async Task<HttpResponseMessage> GetStorageProvider(string subscriptionId)
        {
            var json = await AzureResourceManagerUtil.GetStorageProvider(subscriptionId);
            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(json, Encoding.UTF8, "application/json");
            return response;
        }

        [Route("resources")]
        public async Task<HttpResponseMessage> GetResourcesProvider(string subscriptionId)
        {
            var json = await AzureResourceManagerUtil.GetResourcesProvider(subscriptionId);
            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(json, Encoding.UTF8, "application/json");
            return response;
        }
    }
}
