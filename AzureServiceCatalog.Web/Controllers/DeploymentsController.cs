using AzureServiceCatalog.Web.Models;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace AzureServiceCatalog.Web.Controllers
{
    [RoutePrefix("api/deployments")]
    public class DeploymentsController : ApiController
    {
        private TableRepository repository = new TableRepository();

        public async Task<IHttpActionResult> Post(AscDeployment deployment)
        {
            var securityProcessor = new SecurityProcessor();
            var userHasAccess = securityProcessor.CheckUserPermissionToSubscription(deployment.SubscriptionId);
            if (!userHasAccess)
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }

            var template = await repository.GetTemplate(deployment.TemplateName);
            deployment.Template = template.TemplateData;
            var result = await DeploymentManager.Deploy(deployment);
            return this.Ok(result);
        }

        [Route("validate")]
        public async Task<IHttpActionResult> PostValidate(AscDeployment deployment)
        {
            var template = await repository.GetTemplate(deployment.TemplateName);
            deployment.Template = template.TemplateData;
            var result = await DeploymentManager.ValidateDeployment(deployment);
            return this.Ok(result);
        }

        public async Task<IHttpActionResult> Get(string resourceGroupName, string deploymentName, string subscriptionId)
        {
            var result = await DeploymentManager.GetDeployment(resourceGroupName, deploymentName, subscriptionId);
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
            var result = await DeploymentManager.GetDeploymentList(resourceGroupName, subscriptionId);
            return this.Ok(result);
        }

        [Route("status")]
        public async Task<IHttpActionResult> GetDeploymentOperations(string resourceGroupName, string deploymentName, string subscriptionId)
        {
            var result = await DeploymentManager.GetDeploymentOperations(resourceGroupName, deploymentName, subscriptionId);
            return this.Ok(result);
        }
    }
}
