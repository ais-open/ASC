using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using AzureServiceCatalog.Models;
using AzureServiceCatalog.Helpers;
using AzureServiceCatalog.Web.Models;
using Newtonsoft.Json.Linq;
using System.Web;
using System.Security.Claims;

namespace AzureServiceCatalog.Web.Controllers
{
    [RoutePrefix("api/policy-assignments")]
    [ADGroupAuthorize(SecurityGroupType.CanCreate)]
    public class PolicyAssignmentsController : ApiController
    {
        private PoliciesHelper client = new PoliciesHelper();

        [Route("")]
        public async Task<IHttpActionResult> Get(string subscriptionId)
        {
            var thisOperationContext = new BaseOperationContext("PolicyAssignmentsController:Get")
            {
                IpAddress = HttpContext.Current.Request.UserHostAddress,
                UserId = ClaimsPrincipal.Current.SignedInUserName(),
                UserName = ClaimsPrincipal.Current.Identity.Name
            };
            try
            {
                var policies = await this.client.GetPolicyAssignments(subscriptionId, thisOperationContext);
                var responseMsg = this.Request.CreateResponse(HttpStatusCode.OK);
                responseMsg.Content = policies.ToStringContent();
                IHttpActionResult response = ResponseMessage(responseMsg);
                return response;
            }
            catch (Exception ex)
            {
                TraceHelper.TraceError(thisOperationContext.OperationId, thisOperationContext.OperationName, ex);
                return Content(HttpStatusCode.InternalServerError, JObject.FromObject(ErrorInformation.GetInternalServerErrorInformation()));
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        [Route("{policyAssignmentName}")]
        public async Task<IHttpActionResult> Get(string subscriptionId, string policyAssignmentName)
        {
            var thisOperationContext = new BaseOperationContext("PolicyAssignmentsController:Get")
            {
                IpAddress = HttpContext.Current.Request.UserHostAddress,
                UserId = ClaimsPrincipal.Current.SignedInUserName(),
                UserName = ClaimsPrincipal.Current.Identity.Name
            };
            try
            {
                var policies = await this.client.GetPolicyAssignment(subscriptionId, policyAssignmentName, thisOperationContext);
                var responseMsg = this.Request.CreateResponse(HttpStatusCode.OK);
                responseMsg.Content = policies.ToStringContent();
                IHttpActionResult response = ResponseMessage(responseMsg);
                return response;
            }
            catch (Exception ex)
            {
                TraceHelper.TraceError(thisOperationContext.OperationId, thisOperationContext.OperationName, ex);
                return Content(HttpStatusCode.InternalServerError, JObject.FromObject(ErrorInformation.GetInternalServerErrorInformation()));
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        [Route("{policyAssignmentName}")]
        public async Task<IHttpActionResult> Put(string subscriptionId, string policyAssignmentName, [FromBody]object policy)
        {
            var thisOperationContext = new BaseOperationContext("PolicyAssignmentsController:Put")
            {
                IpAddress = HttpContext.Current.Request.UserHostAddress,
                UserId = ClaimsPrincipal.Current.SignedInUserName(),
                UserName = ClaimsPrincipal.Current.Identity.Name
            };
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
                    var azureResponse = await this.client.SavePolicyAssignment(subscriptionId, policyAssignmentName, policy, thisOperationContext);
                    var responseMsg = this.Request.CreateResponse(HttpStatusCode.OK);
                    responseMsg.Content = azureResponse.ToStringContent();
                    IHttpActionResult response = ResponseMessage(responseMsg);
                    return response;
                }
            }
            catch (Exception ex)
            {
                TraceHelper.TraceError(thisOperationContext.OperationId, thisOperationContext.OperationName, ex);
                return Content(HttpStatusCode.InternalServerError, JObject.FromObject(ErrorInformation.GetInternalServerErrorInformation()));
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        [Route("{policyAssignmentName}")]
        public async Task<IHttpActionResult> Delete(string subscriptionId, string policyAssignmentName)
        {
            var thisOperationContext = new BaseOperationContext("PolicyAssignmentsController:Delete")
            {
                IpAddress = HttpContext.Current.Request.UserHostAddress,
                UserId = ClaimsPrincipal.Current.SignedInUserName(),
                UserName = ClaimsPrincipal.Current.Identity.Name
            };
            try
            {
                await this.client.DeletePolicyAssignment(subscriptionId, policyAssignmentName, thisOperationContext);
                return this.StatusCode(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                TraceHelper.TraceError(thisOperationContext.OperationId, thisOperationContext.OperationName, ex);
                return Content(HttpStatusCode.InternalServerError, JObject.FromObject(ErrorInformation.GetInternalServerErrorInformation()));
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }
    }
}
