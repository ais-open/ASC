using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using AzureServiceCatalog.Helpers;
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
            try
            {
                var json = await AzureResourceManagerUtil.GetStorageProvider(subscriptionId);
                var responseMsg = this.Request.CreateResponse(HttpStatusCode.OK);
                responseMsg.Content = new StringContent(json, Encoding.UTF8, "application/json");
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

        [Route("resources")]
        public async Task<IHttpActionResult> GetResourcesProvider(string subscriptionId)
        {
            try
            {
                var json = await AzureResourceManagerUtil.GetResourcesProvider(subscriptionId);
                var responseMsg = this.Request.CreateResponse(HttpStatusCode.OK);
                responseMsg.Content = new StringContent(json, Encoding.UTF8, "application/json");
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
