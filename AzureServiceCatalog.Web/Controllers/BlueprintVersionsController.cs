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
    public class BlueprintVersionsController : ApiController
    {
        private BlueprintsClient client = new BlueprintsClient();

        [Route("api/show-blueprint-versions/{blueprintName}")]
        public async Task<HttpResponseMessage> Get(string subscriptionId, string blueprintName)
        {
            var blueprintVersions = await this.client.GetBlueprintVersions(subscriptionId, blueprintName);
            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = blueprintVersions.ToStringContent();
            return response;
        }

        [Route("api/get-blueprint-version/{blueprintName}")]
        public async Task<HttpResponseMessage> Get(string subscriptionId, string blueprintName, string versionName)
        {
            var blueprintVersions = await this.client.GetBlueprintVersion(subscriptionId, blueprintName, versionName);
            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = blueprintVersions.ToStringContent();
            return response;
        }
    }
}
