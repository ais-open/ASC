using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using AzureServiceCatalog.Helpers;
using AzureServiceCatalog.Models;

namespace AzureServiceCatalog.Web.Controllers
{
    [RoutePrefix("api/policies")]
    public class PoliciesController : ApiController
    {
        private PoliciesClient client = new PoliciesClient();
        private TableRepository repository = new TableRepository();

        [Route("")]
        public async Task<List<object>> Get(string subscriptionId)
        {
            var policies = await this.client.GetPolicies(subscriptionId);
            var lookupPaths = await this.repository.GetPolicyLookupPaths(subscriptionId);
            var list = new List<object>();
            dynamic pols = JObject.Parse(policies);
            foreach (var item in pols.value)
            {
                var lookupPath = lookupPaths.SingleOrDefault(x => x.RowKey == (string)item.name);
                var policyItem = new {
                    policy = item,
                    policyLookupPath = (lookupPath != null ? lookupPath.PolicyLookupPath : null)
                };
                list.Add(policyItem);
            }
            return list;
        }

        [Route("{definitionName}")]
        public async Task<HttpResponseMessage> Get(string subscriptionId, string definitionName)
        {
            var policy = await this.client.GetPolicy(subscriptionId, definitionName);
            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            var policyLookupPath = await this.repository.GetPolicyLookupPath(subscriptionId, definitionName);
            var responseBody = "{ \"policy\": " + policy + ", \"lookupPath\": \"" + policyLookupPath + "\" }";
            response.Content = responseBody.ToStringContent();
            return response;
        }

        [ADGroupAuthorize(SecurityGroupType.CanCreate)]
        [Route("{definitionName}")]
        public async Task<HttpResponseMessage> Put(string subscriptionId, string definitionName, [FromBody]object policyDefinition)
        {
            dynamic requestBody = policyDefinition;
            await this.repository.SavePolicyLookupPath(subscriptionId, definitionName, (string)requestBody.lookupPath);
            var azureResponse = await this.client.SavePolicy(subscriptionId, definitionName, requestBody.policy);
            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            string responseBody = "{ \"policy\": " + azureResponse + ", \"lookupPath\": \"" + requestBody.lookupPath + "\" }";
            response.Content = responseBody.ToStringContent();
            return response;
        }

        [ADGroupAuthorize(SecurityGroupType.CanCreate)]
        [Route("{definitionName}")]
        public async Task<IHttpActionResult> Delete(string subscriptionId, string definitionName)
        {
            await this.client.DeletePolicy(subscriptionId, definitionName);
            return this.StatusCode(HttpStatusCode.NoContent);
        }
    }
}
