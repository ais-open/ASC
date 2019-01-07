using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Azure.Management.Resources.Models;
using System.Threading;

namespace AzureServiceCatalog.Web.Models
{
    public static class DeploymentManager
    {
        public static async Task<DeploymentOperationsCreateResult> Deploy(AscDeployment deployment)
        {
            var client = Utils.GetResourceManagementClient(deployment.SubscriptionId);
            Deployment d = new Deployment
            {
                Properties = new DeploymentProperties
                {
                    Mode = DeploymentMode.Incremental,
                    Template = deployment.Template,
                    Parameters = deployment.Parameters
                }
            };

            var result = await client.Deployments.CreateOrUpdateAsync(deployment.ResourceGroupName, deployment.DeploymentName,
                parameters: d, cancellationToken: new CancellationToken());
            return result;
        }

        public static async Task<DeploymentValidateResponse> ValidateDeployment(AscDeployment deployment)
        {
            var client = Utils.GetResourceManagementClient(deployment.SubscriptionId);
            Deployment d = new Deployment
            {
                Properties = new DeploymentProperties
                {
                    Mode = DeploymentMode.Incremental,
                    Template = deployment.Template,
                    Parameters = deployment.Parameters
                }
            };

            var result = await client.Deployments.ValidateAsync(deployment.ResourceGroupName, deployment.DeploymentName,
                parameters: d, cancellationToken: new CancellationToken());
            return result;
        }

        public static async Task<DeploymentGetResult> GetDeployment(string resourceGroupName, string deploymentName, string subscriptionId)
        {
            var client = Utils.GetResourceManagementClient(subscriptionId);
            var result = await client.Deployments.GetAsync(resourceGroupName, deploymentName, new CancellationToken());
            return result;
        }

        public static async Task<DeploymentListResult> GetDeploymentList(string resourceGroupName, string subscriptionId)
        {
            var client = Utils.GetResourceManagementClient(subscriptionId);
            var result = await client.Deployments.ListAsync(resourceGroupName, new DeploymentListParameters(), new CancellationToken());
            return result;
        }

        public static async Task<DeploymentOperationsListResult> GetDeploymentOperations(string resourceGroupName, string deploymentName, string subscriptionId)
        {
            var client = Utils.GetResourceManagementClient(subscriptionId);
            var listParams = new DeploymentOperationsListParameters();
            var result = await client.DeploymentOperations.ListAsync(resourceGroupName, deploymentName, listParams, new CancellationToken());
            return result;
        }

        public static async Task<DeploymentOperationsGetResult> GetDeploymentOperationStatus(string resourceGroupName, string deploymentName, string operationId, string subscriptionId)
        {
            var client = Utils.GetResourceManagementClient(subscriptionId);
            var result = await client.DeploymentOperations.GetAsync(resourceGroupName, deploymentName, operationId, new CancellationToken());
            return result;
        }
    }

    public class AscDeployment
    {
        public string ResourceGroupName { get; set; }
        public string DeploymentName { get; set; }
        public string Parameters { get; set; }
        public string Template { get; set; }
        public string TemplateName { get; set; }
        public string SubscriptionId { get; set; }
    }
}