using AzureServiceCatalog.Web.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace AzureServiceCatalog.Web.Controllers
{
    [RoutePrefix("api/show-blueprint-versions")]
    public class BlueprintVersionsController : ApiController
    {
        private BlueprintsClient client = new BlueprintsClient();

        [Route("{blueprintName}")]
        public async Task<HttpResponseMessage> Get(string subscriptionId, string blueprintName)
        {
            var blueprintVersions = await this.client.GetBlueprintVersions(subscriptionId, blueprintName);
            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = blueprintVersions.ToStringContent();
            return response;
        }
    }
}
