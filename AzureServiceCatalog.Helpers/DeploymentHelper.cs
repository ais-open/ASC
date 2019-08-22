using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Azure.Management.Resources.Models;
using System.Threading;
using AzureServiceCatalog.Models;

namespace AzureServiceCatalog.Helpers
{
    public static class DeploymentHelper
    {
        public static async Task<DeploymentOperationsCreateResult> Deploy(AscDeployment deployment, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "DeploymentHelper:Deploy");
            try
            {
                var client = Helpers.GetResourceManagementClient(deployment.SubscriptionId, thisOperationContext);
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
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public static async Task<DeploymentValidateResponse> ValidateDeployment(AscDeployment deployment, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "DeploymentHelper:ValidateDeployment");
            try
            {
                var client = Helpers.GetResourceManagementClient(deployment.SubscriptionId, thisOperationContext);
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
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public static async Task<DeploymentGetResult> GetDeployment(string resourceGroupName, string deploymentName, string subscriptionId, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "DeploymentHelper:GetDeployment");
            try
            {
                var client = Helpers.GetResourceManagementClient(subscriptionId, thisOperationContext);
                var result = await client.Deployments.GetAsync(resourceGroupName, deploymentName, new CancellationToken());
                return result;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public static async Task<DeploymentListResult> GetDeploymentList(string resourceGroupName, string subscriptionId, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "DeploymentHelper:GetDeploymentList");
            try
            {
                var client = Helpers.GetResourceManagementClient(subscriptionId, thisOperationContext);
                var result = await client.Deployments.ListAsync(resourceGroupName, new DeploymentListParameters(), new CancellationToken());
                return result;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }

        }

        public static async Task<DeploymentOperationsListResult> GetDeploymentOperations(string resourceGroupName, string deploymentName, string subscriptionId, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "DeploymentHelper:GetDeploymentOperations");
            try
            {
                var client = Helpers.GetResourceManagementClient(subscriptionId, thisOperationContext);
                var listParams = new DeploymentOperationsListParameters();
                var result = await client.DeploymentOperations.ListAsync(resourceGroupName, deploymentName, listParams, new CancellationToken());
                return result;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public static async Task<DeploymentOperationsGetResult> GetDeploymentOperationStatus(string resourceGroupName, string deploymentName, string operationId, string subscriptionId, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "DeploymentHelper:GetDeploymentOperationStatus");
            try
            {
                var client = Helpers.GetResourceManagementClient(subscriptionId, thisOperationContext);
                var result = await client.DeploymentOperations.GetAsync(resourceGroupName, deploymentName, operationId, new CancellationToken());
                return result;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }
    }
}