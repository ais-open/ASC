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
            var blueprintAssignments = await this.client.GetAssignedBlueprints(subscriptionId);
            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = blueprintAssignments.ToStringContent();
            return response;
        }

        [Route("{assignmentName}")]
        public async Task<HttpResponseMessage> Get(string subscriptionId, string assignmentName)
        {
            var blueprintAssignment = await this.client.GetAssignedBlueprint(subscriptionId, assignmentName);
            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = blueprintAssignment.ToStringContent();
            return response;
        }
    }
}
