using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using AzureServiceCatalog.Models;
using AzureServiceCatalog.Helpers;
using AzureServiceCatalog.Web.Models;
using Newtonsoft.Json.Linq;

namespace AzureServiceCatalog.Web.Controllers
{
    [RoutePrefix("api/policy-assignments")]
    [ADGroupAuthorize(SecurityGroupType.CanCreate)]
    public class PolicyAssignmentsController : ApiController
    {
        private PoliciesClient client = new PoliciesClient();

        [Route("")]
        public async Task<IHttpActionResult> Get(string subscriptionId)
        {
            try
            {
                var policies = await this.client.GetPolicyAssignments(subscriptionId);
                var responseMsg = this.Request.CreateResponse(HttpStatusCode.OK);
                responseMsg.Content = policies.ToStringContent();
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

        [Route("{policyAssignmentName}")]
        public async Task<IHttpActionResult> Get(string subscriptionId, string policyAssignmentName)
        {
            try
            {
                var policies = await this.client.GetPolicyAssignment(subscriptionId, policyAssignmentName);
                var responseMsg = this.Request.CreateResponse(HttpStatusCode.OK);
                responseMsg.Content = policies.ToStringContent();
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

        [Route("{policyAssignmentName}")]
        public async Task<IHttpActionResult> Put(string subscriptionId, string policyAssignmentName, [FromBody]object policy)
        {
            try
            {
                if (policy == null)
                {
                    ErrorInformation errorInformation = new ErrorInformation();
                    errorInformation.Code = "InvalidRequest";
                    errorInformation.Message = "Request body is invalid.";
                    return Content(HttpStatusCode.BadRequest, JObject.FromObject(errorInformation));
                } else
                {
                    var azureResponse = await this.client.SavePolicyAssignment(subscriptionId, policyAssignmentName, policy);
                    var responseMsg = this.Request.CreateResponse(HttpStatusCode.OK);
                    responseMsg.Content = azureResponse.ToStringContent();
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

        [Route("{policyAssignmentName}")]
        public async Task<IHttpActionResult> Delete(string subscriptionId, string policyAssignmentName)
        {
            try
            {
                await this.client.DeletePolicyAssignment(subscriptionId, policyAssignmentName);
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
