using System;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using AzureServiceCatalog.Helpers;
using AzureServiceCatalog.Models;
using AzureServiceCatalog.Web.Models;
using Newtonsoft.Json.Linq;

namespace AzureServiceCatalog.Web.Controllers
{
    [RoutePrefix("api/deployments")]
    public class DeploymentsController : ApiController
    {
        private TableRepository repository = new TableRepository();

        public async Task<IHttpActionResult> Post(AscDeployment deployment)
        {
            var thisOperationContext = new BaseOperationContext("DeploymentsController:Post");
            thisOperationContext.IpAddress = HttpContext.Current.Request.UserHostAddress;
            thisOperationContext.UserId = ClaimsPrincipal.Current.SignedInUserName();
            thisOperationContext.UserName = ClaimsPrincipal.Current.Identity.Name;
            try
            {
                if (deployment == null)
                {
                    ErrorInformation errorInformation = new ErrorInformation();
                    errorInformation.Code = "InvalidRequest";
                    errorInformation.Message = "Request body is invalid.";
                    return Content(HttpStatusCode.BadRequest, JObject.FromObject(errorInformation));
                } else
                {
                    var securityHelper = new SecurityHelper();
                    var userHasAccess = await securityHelper.CheckUserPermissionToSubscription(deployment.SubscriptionId, thisOperationContext);
                    if (!userHasAccess)
                    {
                        ErrorInformation errorInformation = new ErrorInformation();
                        errorInformation.Code = "Forbidden";
                        errorInformation.Message = "Not authorized to perform this action.";
                        return Content(HttpStatusCode.Forbidden, JObject.FromObject(errorInformation));
                    }
                    var template = await repository.GetTemplate(deployment.TemplateName, thisOperationContext);
                    deployment.Template = template.TemplateData;
                    var result = await DeploymentHelper.Deploy(deployment, thisOperationContext);
                    return this.Ok(result);
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

        [Route("validate")]
        public async Task<IHttpActionResult> PostValidate(AscDeployment deployment)
        {
            var thisOperationContext = new BaseOperationContext("DeploymentsController:PostValidate");
            thisOperationContext.IpAddress = HttpContext.Current.Request.UserHostAddress;
            thisOperationContext.UserId = ClaimsPrincipal.Current.SignedInUserName();
            thisOperationContext.UserName = ClaimsPrincipal.Current.Identity.Name;
            try
            {
                if (deployment == null)
                {
                    ErrorInformation errorInformation = new ErrorInformation();
                    errorInformation.Code = "InvalidRequest";
                    errorInformation.Message = "Request body is invalid.";
                    return Content(HttpStatusCode.BadRequest, JObject.FromObject(errorInformation));
                } else
                {
                    var template = await repository.GetTemplate(deployment.TemplateName, thisOperationContext);
                    deployment.Template = template.TemplateData;
                    var result = await DeploymentHelper.ValidateDeployment(deployment, thisOperationContext);
                    return this.Ok(result);
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

        public async Task<IHttpActionResult> Get(string resourceGroupName, string deploymentName, string subscriptionId)
        {
            var thisOperationContext = new BaseOperationContext("DeploymentsController:Get");
            thisOperationContext.IpAddress = HttpContext.Current.Request.UserHostAddress;
            thisOperationContext.UserId = ClaimsPrincipal.Current.SignedInUserName();
            thisOperationContext.UserName = ClaimsPrincipal.Current.Identity.Name;
            try
            {
                var result = await DeploymentHelper.GetDeployment(resourceGroupName, deploymentName, subscriptionId, thisOperationContext);
                var response = new
                {
                    subscriptionId = subscriptionId,
                    provisioningState = result.Deployment.Properties.ProvisioningState,
                    correlationId = result.Deployment.Properties.CorrelationId
                };
                return this.Ok(response);
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

        [Route("list")]
        public async Task<IHttpActionResult> GetDeploymentList(string resourceGroupName, string subscriptionId)
        {
            var thisOperationContext = new BaseOperationContext("DeploymentsController:GetDeploymentList");
            thisOperationContext.IpAddress = HttpContext.Current.Request.UserHostAddress;
            thisOperationContext.UserId = ClaimsPrincipal.Current.SignedInUserName();
            thisOperationContext.UserName = ClaimsPrincipal.Current.Identity.Name;
            try
            {
                var result = await DeploymentHelper.GetDeploymentList(resourceGroupName, subscriptionId, thisOperationContext);
                return this.Ok(result);
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

        [Route("status")]
        public async Task<IHttpActionResult> GetDeploymentOperations(string resourceGroupName, string deploymentName, string subscriptionId)
        {
            var thisOperationContext = new BaseOperationContext("DeploymentsController:GetDeploymentOperations");
            thisOperationContext.IpAddress = HttpContext.Current.Request.UserHostAddress;
            thisOperationContext.UserId = ClaimsPrincipal.Current.SignedInUserName();
            thisOperationContext.UserName = ClaimsPrincipal.Current.Identity.Name;
            try
            {
                var result = await DeploymentHelper.GetDeploymentOperations(resourceGroupName, deploymentName, subscriptionId, thisOperationContext);
                return this.Ok(result);
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
