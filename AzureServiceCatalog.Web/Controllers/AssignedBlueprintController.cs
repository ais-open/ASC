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
    [RoutePrefix("api/blueprintAssignments")]
    public class AssignedBlueprintController : ApiController
    {
        private BlueprintsClient client = new BlueprintsClient();

        [Route("")]
        public async Task<HttpResponseMessage> Get(string subscriptionId)
        {
            var policies = await this.client.GetAssignedBlueprints(subscriptionId);
            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = policies.ToStringContent();
            return response;
        }
    }
}
