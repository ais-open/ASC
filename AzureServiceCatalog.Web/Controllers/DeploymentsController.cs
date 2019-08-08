using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using AzureServiceCatalog.Helpers;

namespace AzureServiceCatalog.Web.Controllers
{
    [RoutePrefix("api/deployments")]
    public class DeploymentsController : ApiController
    {
        private TableRepository repository = new TableRepository();

        public async Task<IHttpActionResult> Post(AscDeployment deployment)
        {
            var securityHelper = new SecurityHelper();
            var userHasAccess = await securityHelper.CheckUserPermissionToSubscription(deployment.SubscriptionId);
            if (!userHasAccess)
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }

            var template = await repository.GetTemplate(deployment.TemplateName);
            deployment.Template = template.TemplateData;
            var result = await DeploymentHelper.Deploy(deployment);
            return this.Ok(result);
        }

        [Route("validate")]
        public async Task<IHttpActionResult> PostValidate(AscDeployment deployment)
        {
            var template = await repository.GetTemplate(deployment.TemplateName);
            deployment.Template = template.TemplateData;
            var result = await DeploymentHelper.ValidateDeployment(deployment);
            return this.Ok(result);
        }

        public async Task<IHttpActionResult> Get(string resourceGroupName, string deploymentName, string subscriptionId)
        {
            var result = await DeploymentHelper.GetDeployment(resourceGroupName, deploymentName, subscriptionId);
            var response = new
            {
                subscriptionId = subscriptionId,
                provisioningState = result.Deployment.Properties.ProvisioningState,
                correlationId = result.Deployment.Properties.CorrelationId
            };
            return this.Ok(response);
        }

        [Route("list")]
        public async Task<IHttpActionResult> GetDeploymentList(string resourceGroupName, string subscriptionId)
        {
            var result = await DeploymentHelper.GetDeploymentList(resourceGroupName, subscriptionId);
            return this.Ok(result);
        }

        [Route("status")]
        public async Task<IHttpActionResult> GetDeploymentOperations(string resourceGroupName, string deploymentName, string subscriptionId)
        {
            var result = await DeploymentHelper.GetDeploymentOperations(resourceGroupName, deploymentName, subscriptionId);
            return this.Ok(result);
        }
    }
}
