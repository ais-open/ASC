using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using AzureServiceCatalog.Models;
using AzureServiceCatalog.Helpers;

namespace AzureServiceCatalog.Web.Controllers
{
    [RoutePrefix("api/policy-assignments")]
    [ADGroupAuthorize(SecurityGroupType.CanCreate)]
    public class PolicyAssignmentsController : ApiController
    {
        private PoliciesClient client = new PoliciesClient();

        [Route("")]
        public async Task<HttpResponseMessage> Get(string subscriptionId)
        {
            var policies = await this.client.GetPolicyAssignments(subscriptionId);
            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = policies.ToStringContent();
            return response;
        }

        [Route("{policyAssignmentName}")]
        public async Task<HttpResponseMessage> Get(string subscriptionId, string policyAssignmentName)
        {
            var policies = await this.client.GetPolicyAssignment(subscriptionId, policyAssignmentName);
            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = policies.ToStringContent();
            return response;
        }

        [Route("{policyAssignmentName}")]
        public async Task<HttpResponseMessage> Put(string subscriptionId, string policyAssignmentName, [FromBody]object policy)
        {
            var azureResponse = await this.client.SavePolicyAssignment(subscriptionId, policyAssignmentName, policy);
            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = azureResponse.ToStringContent();
            return response;
        }

        [Route("{policyAssignmentName}")]
        public async Task<IHttpActionResult> Delete(string subscriptionId, string policyAssignmentName)
        {
            await this.client.DeletePolicyAssignment(subscriptionId, policyAssignmentName);
            return this.StatusCode(HttpStatusCode.NoContent);
        }
    }
}
