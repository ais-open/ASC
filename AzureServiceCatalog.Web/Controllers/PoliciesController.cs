using System;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using AzureServiceCatalog.Helpers;
using AzureServiceCatalog.Models;
using AzureServiceCatalog.Web.Models;

namespace AzureServiceCatalog.Web.Controllers
{
    [RoutePrefix("api/policies")]
    public class PoliciesController : ApiController
    {
        private PoliciesClient client = new PoliciesClient();
        private TableRepository repository = new TableRepository();

        [Route("")]
        public async Task<IHttpActionResult> Get(string subscriptionId)
        {
            try
            {
                var policies = await this.client.GetPolicies(subscriptionId);
                var lookupPaths = await this.repository.GetPolicyLookupPaths(subscriptionId);
                var list = new List<object>();
                dynamic pols = JObject.Parse(policies);
                foreach (var item in pols.value)
                {
                    var lookupPath = lookupPaths.SingleOrDefault(x => x.RowKey == (string)item.name);
                    var policyItem = new
                    {
                        policy = item,
                        policyLookupPath = (lookupPath != null ? lookupPath.PolicyLookupPath : null)
                    };
                    list.Add(policyItem);
                }
                return this.Ok(list);
            }
            catch (Exception)
            {
                return Content(HttpStatusCode.InternalServerError, JObject.FromObject(ErrorInformation.GetInternalServerErrorInformation()));
            }
            finally
            {

            }
        }

        [Route("{definitionName}")]
        public async Task<IHttpActionResult> Get(string subscriptionId, string definitionName)
        {
            try
            {
                var policy = await this.client.GetPolicy(subscriptionId, definitionName);
                var responseMsg = this.Request.CreateResponse(HttpStatusCode.OK);
                var policyLookupPath = await this.repository.GetPolicyLookupPath(subscriptionId, definitionName);
                var responseBody = "{ \"policy\": " + policy + ", \"lookupPath\": \"" + policyLookupPath + "\" }";
                responseMsg.Content = responseBody.ToStringContent();
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

        [ADGroupAuthorize(SecurityGroupType.CanCreate)]
        [Route("{definitionName}")]
        public async Task<IHttpActionResult> Put(string subscriptionId, string definitionName, [FromBody]object policyDefinition)
        {
            try
            {
                if (policyDefinition == null)
                {
                    ErrorInformation errorInformation = new ErrorInformation();
                    errorInformation.Code = "InvalidRequest";
                    errorInformation.Message = "Request body is invalid.";
                    return Content(HttpStatusCode.BadRequest, JObject.FromObject(errorInformation));
                } else
                {
                    dynamic requestBody = policyDefinition;
                    await this.repository.SavePolicyLookupPath(subscriptionId, definitionName, (string)requestBody.lookupPath);
                    var azureResponse = await this.client.SavePolicy(subscriptionId, definitionName, requestBody.policy);
                    var responseMsg = this.Request.CreateResponse(HttpStatusCode.OK);
                    string responseBody = "{ \"policy\": " + azureResponse + ", \"lookupPath\": \"" + requestBody.lookupPath + "\" }";
                    responseMsg.Content = responseBody.ToStringContent();
                    IHttpActionResult response = ResponseMessage(responseMsg);
                    return response;
                }
            }
            catch (Exception)
            {
                return Content(HttpStatusCode.InternalServerError, JObject.FromObject(ErrorInformation.GetInternalServerErrorInformation()));
            }
            finally
            {

            }
        }

        [ADGroupAuthorize(SecurityGroupType.CanCreate)]
        [Route("{definitionName}")]
        public async Task<IHttpActionResult> Delete(string subscriptionId, string definitionName)
        {
            try
            {
                await this.client.DeletePolicy(subscriptionId, definitionName);
                return this.StatusCode(HttpStatusCode.NoContent);
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
