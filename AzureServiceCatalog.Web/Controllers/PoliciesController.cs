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
using System.Web;
using System.Security.Claims;

namespace AzureServiceCatalog.Web.Controllers
{
    [RoutePrefix("api/policies")]
    public class PoliciesController : ApiController
    {
        private PoliciesHelper client = new PoliciesHelper();
        private TableRepository repository = new TableRepository();

        [Route("")]
        public async Task<IHttpActionResult> Get(string subscriptionId)
        {
            var thisOperationContext = new BaseOperationContext("PoliciesController:Get")
            {
                IpAddress = HttpContext.Current.Request.UserHostAddress,
                UserId = ClaimsPrincipal.Current.SignedInUserName(),
                UserName = ClaimsPrincipal.Current.Identity.Name
            };
            try
            {
                var policies = await this.client.GetPolicies(subscriptionId, thisOperationContext);
                var lookupPaths = await this.repository.GetPolicyLookupPaths(subscriptionId, thisOperationContext);
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

        [Route("{definitionName}")]
        public async Task<IHttpActionResult> Get(string subscriptionId, string definitionName)
        {
            var thisOperationContext = new BaseOperationContext("PoliciesController:Get")
            {
                IpAddress = HttpContext.Current.Request.UserHostAddress,
                UserId = ClaimsPrincipal.Current.SignedInUserName(),
                UserName = ClaimsPrincipal.Current.Identity.Name
            };
            try
            {
                var policy = await this.client.GetPolicy(subscriptionId, definitionName, thisOperationContext);
                var responseMsg = this.Request.CreateResponse(HttpStatusCode.OK);
                var policyLookupPath = await this.repository.GetPolicyLookupPath(subscriptionId, definitionName, thisOperationContext);
                var responseBody = "{ \"policy\": " + policy + ", \"lookupPath\": \"" + policyLookupPath + "\" }";
                responseMsg.Content = responseBody.ToStringContent();
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

        [ADGroupAuthorize(SecurityGroupType.CanCreate)]
        [Route("{definitionName}")]
        public async Task<IHttpActionResult> Put(string subscriptionId, string definitionName, [FromBody]object policyDefinition)
        {
            var thisOperationContext = new BaseOperationContext("PoliciesController:Put")
            {
                IpAddress = HttpContext.Current.Request.UserHostAddress,
                UserId = ClaimsPrincipal.Current.SignedInUserName(),
                UserName = ClaimsPrincipal.Current.Identity.Name
            };
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
                    await this.repository.SavePolicyLookupPath(subscriptionId, definitionName, (string)requestBody.lookupPath, thisOperationContext);
                    var azureResponse = await this.client.SavePolicy(subscriptionId, definitionName, requestBody.policy, thisOperationContext);
                    var responseMsg = this.Request.CreateResponse(HttpStatusCode.OK);
                    string responseBody = "{ \"policy\": " + azureResponse + ", \"lookupPath\": \"" + requestBody.lookupPath + "\" }";
                    responseMsg.Content = responseBody.ToStringContent();
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

        [ADGroupAuthorize(SecurityGroupType.CanCreate)]
        [Route("{definitionName}")]
        public async Task<IHttpActionResult> Delete(string subscriptionId, string definitionName)
        {
            var thisOperationContext = new BaseOperationContext("PoliciesController:Delete")
            {
                IpAddress = HttpContext.Current.Request.UserHostAddress,
                UserId = ClaimsPrincipal.Current.SignedInUserName(),
                UserName = ClaimsPrincipal.Current.Identity.Name
            };
            try
            {
                await this.client.DeletePolicy(subscriptionId, definitionName, thisOperationContext);
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
