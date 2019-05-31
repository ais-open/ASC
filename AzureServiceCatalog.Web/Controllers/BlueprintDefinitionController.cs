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
    [RoutePrefix("api/blueprintDefinitions")]
    public class BlueprintDefinitionController : ApiController
    {
        private BlueprintsClient client = new BlueprintsClient();

        [Route("")]
        public async Task<HttpResponseMessage> Get(string subscriptionId)
        {
            var blueprints = await this.client.GetBlueprintDefinitions(subscriptionId);
            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = blueprints.ToStringContent();
            return response;
        }
    }
}
