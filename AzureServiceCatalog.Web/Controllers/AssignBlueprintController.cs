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
        public async Task<HttpResponseMessage> Put(string subscriptionId, string assignmentName, [FromBody]object blueprintAssignment)
        {
            dynamic requestBody = blueprintAssignment;
            var azureResponse = await this.client.AssignBlueprint(subscriptionId, assignmentName, requestBody);
            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            string responseBody = "{ \"blueprintAssignment\": " + azureResponse + " }";
            response.Content = responseBody.ToStringContent();
            return response;
        }
    }
}