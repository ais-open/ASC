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
    [RoutePrefix("api/assign-blueprint")]
    public class AssignBlueprintController : ApiController
    {
        private BlueprintsClient client = new BlueprintsClient();

        [Route("{assignmentName}")]
        public async Task<HttpResponseMessage> Get(string subscriptionId, string assignmentName, object blueprintAssignment)
        {
            var assignment = await this.client.AssignBlueprint(subscriptionId, assignmentName, blueprintAssignment);
            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = assignment.ToStringContent();
            return response;
        }
    }
}
