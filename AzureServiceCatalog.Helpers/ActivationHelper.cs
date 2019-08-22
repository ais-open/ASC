using Microsoft.Azure.Management.Resources.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Net.Http;
using Newtonsoft.Json;
using AzureServiceCatalog.Models;

namespace AzureServiceCatalog.Helpers
{
    public class ActivationHelper
    {
        private string storageName;
        private TableCoreRepository coreRepository = new TableCoreRepository();
        private TableRepository repository = new TableRepository();

        public async Task SaveActivation(ActivationInfo activationInfo, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "ActivationHelper: SaveActivation");
            try
            {
                var defaultAdGroup = Config.DefaultAdGroup;
                ADGroup orgGroup = await AzureADGraphApiHelper.CheckIfADGroupExistsByOrgName(activationInfo.Organization.Id, defaultAdGroup, thisOperationContext);

                if (orgGroup == null)
                {
                    await AzureADGraphApiHelper.CreateGroup(activationInfo.Organization.Id, defaultAdGroup, thisOperationContext);
                    orgGroup = await AzureADGraphApiHelper.CheckIfADGroupExistsByOrgName(activationInfo.Organization.Id, defaultAdGroup, thisOperationContext);
                }

                if (orgGroup == null)
                {
                    throw new UnauthorizedAccessException($"Default ADGroup: {defaultAdGroup} could not be created! Make sure you have ADMIN access to the Azure AD");
                }

                //var orgGroups = await AzureADGraphApiHelper.GetAllGroupsForOrganization(activationInfo.Organization.Id);
                activationInfo.Organization.CreateProductGroup = orgGroup.Id; //orgGroups[0].Id;
                activationInfo.Organization.AdminGroup = orgGroup.Id; //orgGroups[0].Id;
                activationInfo.Organization.DeployGroup = orgGroup.Id; //orgGroups[0].Id;

                await SaveHostSubscription(activationInfo, parentOperationContext);
                await SaveEnrolledSubscriptions(activationInfo, parentOperationContext);
                await this.coreRepository.SaveOrganization(activationInfo.Organization, thisOperationContext);
                await AddEnrollingUserToAllGroups(activationInfo.Organization, thisOperationContext);
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("ASC");
                const string queryUrl = "https://raw.githubusercontent.com/Azure/azure-quickstart-templates/master/101-vm-simple-windows/azuredeploy.json";
                // Initially set the nextLink to the origin URL
                TemplateViewModel templateInit = new TemplateViewModel();
                var response = await httpClient.GetStringAsync(queryUrl);
                templateInit.Name = "Simple Window VM";
                templateInit.IsPublished = true;
                templateInit.TemplateData = response;
                TemplateViewModel savedTemplateEntity = await repository.SaveTemplate(templateInit, thisOperationContext);
                const string queryUrl2 = "https://raw.githubusercontent.com/Azure/azure-quickstart-templates/master/101-create-ase-with-webapp/azuredeploy.parameters.json";
                // Initially set the nextLink to the origin URL
                TemplateViewModel templateInit2 = new TemplateViewModel();
                var response2 = await httpClient.GetStringAsync(queryUrl2);
                templateInit2.Name = "Web App with Redis Cache and SQL Database";
                templateInit2.IsPublished = true;
                templateInit2.TemplateData = response;
                TemplateViewModel savedTemplateEntity2 = await repository.SaveTemplate(templateInit2, thisOperationContext);

                var isRunningInAzureGeneral = Config.IsRunningInAzureGeneral(Config.StorageAccountEndpointSuffix);
                if (isRunningInAzureGeneral)
                {
                    var notificationHelper = new NotificationHelper();
                    notificationHelper.SendActivationNotification(activationInfo.Organization, thisOperationContext);
                }
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public async Task SaveEnrolledSubscriptions(SubscriptionsViewModel subscriptionsVM, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "ActivationHelper: SaveEnrolledSubscriptions");
            try
            {
                var tenantId = ClaimsPrincipal.Current.TenantId();
                var organization = await this.coreRepository.GetOrganization(tenantId, thisOperationContext);
                foreach (var subscription in subscriptionsVM.Subscriptions)
                {
                    var subscriptionFromDb = this.coreRepository.GetSubscription(subscription.Id, thisOperationContext);
                    var isCurrentlyEnrolled = (subscriptionFromDb != null && subscriptionFromDb.IsEnrolled);
                    var isCurrentlyConnectedHost = (subscriptionFromDb != null && subscriptionFromDb.IsConnected);
                    var isSwitchingEnrollmentOn = (subscription.IsEnrolled && !isCurrentlyEnrolled);
                    var isSwitchingEnrollmentOff = (!subscription.IsEnrolled && isCurrentlyEnrolled);
                    var isSwitchingHostingOn = (subscription.IsConnected && !isCurrentlyConnectedHost);
                    var isSwitchingHostingOff = (!subscription.IsConnected && isCurrentlyConnectedHost);

                    if (isSwitchingEnrollmentOn || isSwitchingHostingOn)
                    {
                        Run.WithProgressBackOff(5, 1, 5, async () =>
                        {
                            await AzureResourceManagerHelper.GrantRoleToServicePrincipalOnSubscriptionAsync(organization.ObjectIdOfCloudSenseServicePrincipal, subscription.Id, thisOperationContext);
                        });
                    }

                    if (isSwitchingEnrollmentOff || isSwitchingHostingOff)
                    {
                        if (subscriptionsVM.Subscriptions.Count == 1)
                        {
                            throw new ValidationException("There must be at least one enrolled and one hosting subscription.");
                        }
                        int numberOfEnrolledSubscriptions = subscriptionsVM.Subscriptions.Count(s => s.IsEnrolled);
                        if (numberOfEnrolledSubscriptions == 0)
                        {
                            throw new ValidationException("There must be at least one enrolled subscription.");
                        }
                        subscription.ContributorGroups = null;
                        await AzureResourceManagerHelper.RevokeRoleFromServicePrincipalOnSubscription(organization.ObjectIdOfCloudSenseServicePrincipal, subscription.Id, thisOperationContext);
                    }

                    if (isSwitchingHostingOn)
                    {
                        // Create Resource Group to hold Storage Account
                        var client = Helpers.GetResourceManagementClient(subscription.Id, thisOperationContext);
                        var rg = new ResourceGroup(subscriptionsVM.Location);
                        var result = await client.ResourceGroups.CreateOrUpdateAsync(subscriptionsVM.ResourceGroup, rg, new CancellationToken());

                        // Create Storage Account
                        var storageName = await AzureResourceManagerHelper.CreateServiceCatalogMetadataStorageAccount(subscription.Id, subscriptionsVM.ResourceGroup, thisOperationContext);
                        string key = await AzureResourceManagerHelper.GetStorageAccountKeysArm(subscription.Id, storageName, thisOperationContext);

                        BlobHelpers.CreateInitialTablesAndBlobContainers(storageName, key, thisOperationContext);

                        subscription.StorageName = storageName;
                        subscription.ConnectedOn = DateTime.Now;
                        subscription.ConnectedBy = ClaimsPrincipal.Current.Identity.Name;

                        CacheDetails(subscription.Id, key, storageName, subscription.OrganizationId, thisOperationContext);
                    }

                    if (subscription.ConnectedOn.IsMinDate())
                    {
                        subscription.ConnectedOn = DateTime.Now;
                    }

                    await this.coreRepository.SaveSubscription(subscription, thisOperationContext);
                }
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        #region Private Methods

        private async Task SaveHostSubscription(ActivationInfo activationInfo, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "ActivationHelper:SaveHostSubscription");
            try
            {
                if (string.IsNullOrEmpty(activationInfo.HostSubscription.ServicePrincipalId))
                {
                    var organizations = await AzureResourceManagerHelper.GetUserOrganizations(thisOperationContext);
                    var selectedOrganization = organizations.Where(o => o.Id == activationInfo.HostSubscription.OrganizationId).FirstOrDefault();
                    activationInfo.HostSubscription.ServicePrincipalId = selectedOrganization.ObjectIdOfCloudSenseServicePrincipal;
                }
                Run.WithProgressBackOff(5, 1, 5, async () =>
                {
                    await AzureResourceManagerHelper.GrantRoleToServicePrincipalOnSubscriptionAsync(activationInfo.HostSubscription.ServicePrincipalId, activationInfo.HostSubscription.SubscriptionId, thisOperationContext);
                });

                // Create Resource Group to hold Storage Account
                string resourceGroup = Config.DefaultResourceGroup;
                var json = await AzureResourceManagerHelper.GetStorageProvider(activationInfo.HostSubscription.SubscriptionId, thisOperationContext);
                JObject storageProvider = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(json);
                var client = Helpers.GetResourceManagementClient(activationInfo.HostSubscription.SubscriptionId, thisOperationContext);
                string location = storageProvider["resourceTypes"][0]["locations"][0].ToString();
                var rg = new ResourceGroup(location);
                var result = await client.ResourceGroups.CreateOrUpdateAsync(resourceGroup, rg, new CancellationToken());


                // Create Storage Account
                this.storageName = await AzureResourceManagerHelper.CreateServiceCatalogMetadataStorageAccount(activationInfo.HostSubscription.SubscriptionId, resourceGroup, thisOperationContext);
                string key = await AzureResourceManagerHelper.GetStorageAccountKeysArm(activationInfo.HostSubscription.SubscriptionId, this.storageName, thisOperationContext);

                BlobHelpers.CreateInitialTablesAndBlobContainers(storageName, key, thisOperationContext);
                CacheDetails(activationInfo.HostSubscription.SubscriptionId, key, storageName, activationInfo.HostSubscription.OrganizationId, thisOperationContext);
                var orgGroups = await AzureADGraphApiHelper.GetAllGroupsForOrganization(activationInfo.Organization.Id, parentOperationContext);
                ContributorGroup[] contributorGroups = new ContributorGroup[1];
                contributorGroups[0] = new ContributorGroup();
                contributorGroups[0].Id = orgGroups[0].Id;
                contributorGroups[0].Name = orgGroups[0].Name;

                var jsonContributorGroups = JsonConvert.SerializeObject(contributorGroups);
                await this.coreRepository.SaveSubscription(new Subscription
                {
                    Id = activationInfo.HostSubscription.SubscriptionId,
                    IsConnected = true,
                    ConnectedOn = DateTime.Now,
                    ContributorGroups = jsonContributorGroups,
                    DisplayName = activationInfo.HostSubscription.SubscriptionName,
                    OrganizationId = activationInfo.HostSubscription.OrganizationId,
                    StorageName = storageName,
                    ConnectedBy = ClaimsPrincipal.Current.Identity.Name,
                }, thisOperationContext);
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        private async Task SaveEnrolledSubscriptions(ActivationInfo activationInfo, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "ActivationHelper:SaveEnrolledSubscriptions");
            try
            {
                foreach (var enrolledSubscription in activationInfo.EnrolledSubscriptions)
                {
                    Run.WithProgressBackOff(5, 1, 5, async () =>
                    {
                        await AzureResourceManagerHelper.GrantRoleToServicePrincipalOnSubscriptionAsync(activationInfo.HostSubscription.ServicePrincipalId, enrolledSubscription.SubscriptionId, thisOperationContext);
                    });

                    var subscriptionFromDb = this.coreRepository.GetSubscription(enrolledSubscription.SubscriptionId, thisOperationContext);
                    Subscription subscriptionToSave = null;
                    if (subscriptionFromDb == null)
                    {
                        subscriptionToSave = new Subscription
                        {
                            Id = enrolledSubscription.SubscriptionId,
                            IsEnrolled = true,
                            ConnectedOn = DateTime.Now,
                            DisplayName = enrolledSubscription.SubscriptionName,
                            OrganizationId = enrolledSubscription.OrganizationId,
                            StorageName = storageName,
                            ConnectedBy = ClaimsPrincipal.Current.Identity.Name,
                        };
                    }
                    else
                    {
                        subscriptionToSave = subscriptionFromDb;
                        subscriptionToSave.IsEnrolled = true;
                    }
                    await this.coreRepository.SaveSubscription(subscriptionToSave, thisOperationContext);
                }
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }  
        }

        private static void CacheDetails(string subscriptionId, string storageKey, string storageName, string organizationId, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "ActivationHelper:CacheDetails");
            try
            {
                string signedInUserUniqueId = ClaimsPrincipal.Current.SignedInUserName();
                if (!string.IsNullOrEmpty(storageName) && !string.IsNullOrEmpty(storageKey))
                {
                    CacheUserDetails cud = new CacheUserDetails();
                    cud.SubscriptionId = subscriptionId;
                    cud.StorageName = storageName;
                    cud.OrganizationId = organizationId;
                    cud.StorageKey = storageKey;
                    MemoryCacher.Delete(signedInUserUniqueId, thisOperationContext);
                    MemoryCacher.Add(signedInUserUniqueId, cud, DateTime.Now.AddMinutes(15), thisOperationContext);
                }
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);

            }
        }

        private static async Task AddEnrollingUserToAllGroups(Organization org, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "ActivationHelper:AddEnrollingUserToAllGroups");
            try
            {
                var userId = ClaimsPrincipal.Current.UserId();
                await AzureADGraphApiHelper.AddUserToGroup(org.Id, org.CreateProductGroup, userId, thisOperationContext);
                if (org.DeployGroup != org.CreateProductGroup)
                {
                    await AzureADGraphApiHelper.AddUserToGroup(org.Id, org.DeployGroup, userId, thisOperationContext);
                }
                if (org.AdminGroup != org.CreateProductGroup)
                {
                    await AzureADGraphApiHelper.AddUserToGroup(org.Id, org.AdminGroup, userId, thisOperationContext);
                }
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        #endregion
    }
}