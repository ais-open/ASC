using Microsoft.Azure.Management.Resources.Models;
using AzureServiceCatalog.Web.Controllers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Script.Serialization;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Net.Http;
using Newtonsoft.Json;

namespace AzureServiceCatalog.Web.Models
{
    public class ActivationProcessor
    {
        private string storageName;
        private TableCoreRepository coreRepository = new TableCoreRepository();
        private TableRepository repository = new TableRepository();

        public async Task SaveActivation(ActivationInfo activationInfo)
        {
            var defaultAdGroup = Config.DefaultAdGroup;
            ADGroup orgGroup = AzureADGraphApiUtil.CheckIfADGroupExistsByOrgName(activationInfo.Organization.Id, defaultAdGroup);

            if (orgGroup == null)
            {
                await AzureADGraphApiUtil.CreateGroup(activationInfo.Organization.Id, defaultAdGroup);
                orgGroup = AzureADGraphApiUtil.CheckIfADGroupExistsByOrgName(activationInfo.Organization.Id, defaultAdGroup);
            }

            if (orgGroup == null)
            {
                throw new UnauthorizedAccessException($"Default ADGroup: {defaultAdGroup} could not be created! Make sure you have ADMIN access to the Azure AD");
            }

            //var orgGroups = AzureADGraphApiUtil.GetAllGroupsForOrganization(activationInfo.Organization.Id);
            activationInfo.Organization.CreateProductGroup = orgGroup.Id; //orgGroups[0].Id;
            activationInfo.Organization.AdminGroup = orgGroup.Id; //orgGroups[0].Id;
            activationInfo.Organization.DeployGroup = orgGroup.Id; //orgGroups[0].Id;

            await SaveHostSubscription(activationInfo);
            await SaveEnrolledSubscriptions(activationInfo);
            await this.coreRepository.SaveOrganization(activationInfo.Organization);
            await AddEnrollingUserToAllGroups(activationInfo.Organization);
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("ASC");
            const string queryUrl = "https://raw.githubusercontent.com/Azure/azure-quickstart-templates/master/101-vm-simple-windows/azuredeploy.json";
            // Initially set the nextLink to the origin URL
            TemplateViewModel templateInit = new TemplateViewModel();
            var response = await httpClient.GetStringAsync(queryUrl);
            templateInit.Name = "Simple Window VM";
            templateInit.IsPublished = true;
            templateInit.TemplateData = response;
            TemplateViewModel savedTemplateEntity = await repository.SaveTemplate(templateInit);
            const string queryUrl2 = "https://raw.githubusercontent.com/Azure/azure-quickstart-templates/master/101-create-ase-with-webapp/azuredeploy.parameters.json";
            // Initially set the nextLink to the origin URL
            TemplateViewModel templateInit2 = new TemplateViewModel();
            var response2 = await httpClient.GetStringAsync(queryUrl2);
            templateInit2.Name = "Web App with Redis Cache and SQL Database";
            templateInit2.IsPublished = true;
            templateInit2.TemplateData = response;
            TemplateViewModel savedTemplateEntity2 = await repository.SaveTemplate(templateInit2);

            var isRunningInAzureGeneral = Config.IsRunningInAzureGeneral(Config.StorageAccountEndpointSuffix);
            if (isRunningInAzureGeneral)
            {
                var notifProcessor = new NotificationProcessor();
                notifProcessor.SendActivationNotification(activationInfo.Organization);
            }

        }

        public async Task SaveEnrolledSubscriptions(SubscriptionsViewModel subscriptionsVM)
        {
            var tenantId = ClaimsPrincipal.Current.TenantId();
            var organization = await this.coreRepository.GetOrganization(tenantId);
            foreach (var subscription in subscriptionsVM.Subscriptions)
            {
                var subscriptionFromDb = this.coreRepository.GetSubscription(subscription.Id);
                var isCurrentlyEnrolled = (subscriptionFromDb != null && subscriptionFromDb.IsEnrolled);
                var isCurrentlyConnectedHost = (subscriptionFromDb != null && subscriptionFromDb.IsConnected);
                var isSwitchingEnrollmentOn = (subscription.IsEnrolled && !isCurrentlyEnrolled);
                var isSwitchingEnrollmentOff = (!subscription.IsEnrolled && isCurrentlyEnrolled);
                var isSwitchingHostingOn = (subscription.IsConnected && !isCurrentlyConnectedHost);
                var isSwitchingHostingOff = (!subscription.IsConnected && isCurrentlyConnectedHost);

                if (isSwitchingEnrollmentOn || isSwitchingHostingOn)
                {
                    Run.WithProgressBackOff(5, 1, 5, () =>
                    {
                        AzureResourceManagerUtil.GrantRoleToServicePrincipalOnSubscription(organization.ObjectIdOfCloudSenseServicePrincipal, subscription.Id);
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
                    AzureResourceManagerUtil.RevokeRoleFromServicePrincipalOnSubscription(organization.ObjectIdOfCloudSenseServicePrincipal, subscription.Id);
                }

                if (isSwitchingHostingOn)
                {
                    // Create Resource Group to hold Storage Account
                    var client = Utils.GetResourceManagementClient(subscription.Id);
                    var rg = new ResourceGroup(subscriptionsVM.Location);
                    var result = await client.ResourceGroups.CreateOrUpdateAsync(subscriptionsVM.ResourceGroup, rg, new CancellationToken());

                    // Create Storage Account
                    var storageName = await AzureResourceManagerUtil.CreateServiceCatalogMetadataStorageAccount(subscription.Id, subscriptionsVM.ResourceGroup);
                    string key = await AzureResourceManagerUtil.GetStorageAccountKeysArm(subscription.Id, storageName);

                    BlobHelpers.CreateInitialTablesAndBlobContainers(storageName, key);

                    subscription.StorageName = storageName;
                    subscription.ConnectedOn = DateTime.Now;
                    subscription.ConnectedBy = ClaimsPrincipal.Current.Identity.Name;

                    CacheDetails(subscription.Id, key, storageName, subscription.OrganizationId);
                }

                if (subscription.ConnectedOn.IsMinDate())
                {
                    subscription.ConnectedOn = DateTime.Now;
                }

                await this.coreRepository.SaveSubscription(subscription);
            }
        }

        #region Private Methods

        private async Task SaveHostSubscription(ActivationInfo activationInfo)
        {
            if (string.IsNullOrEmpty(activationInfo.HostSubscription.ServicePrincipalId))
            {
                var organizations = AzureResourceManagerUtil.GetUserOrganizations();
                var selectedOrganization = organizations.Where(o => o.Id == activationInfo.HostSubscription.OrganizationId).FirstOrDefault();
                activationInfo.HostSubscription.ServicePrincipalId = selectedOrganization.ObjectIdOfCloudSenseServicePrincipal;
            }
            Run.WithProgressBackOff(5, 1, 5, () =>
            {
                AzureResourceManagerUtil.GrantRoleToServicePrincipalOnSubscription(activationInfo.HostSubscription.ServicePrincipalId, activationInfo.HostSubscription.SubscriptionId);
            });

            // Create Resource Group to hold Storage Account
            string resourceGroup = Config.DefaultResourceGroup;
            var json = await AzureResourceManagerUtil.GetStorageProvider(activationInfo.HostSubscription.SubscriptionId);
            JObject storageProvider = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(json);
            var client = Utils.GetResourceManagementClient(activationInfo.HostSubscription.SubscriptionId);
            string location = storageProvider["resourceTypes"][0]["locations"][0].ToString();
            var rg = new ResourceGroup(location);
            var result = await client.ResourceGroups.CreateOrUpdateAsync(resourceGroup, rg, new CancellationToken());


            // Create Storage Account
            this.storageName = await AzureResourceManagerUtil.CreateServiceCatalogMetadataStorageAccount(activationInfo.HostSubscription.SubscriptionId, resourceGroup);
            string key = await AzureResourceManagerUtil.GetStorageAccountKeysArm(activationInfo.HostSubscription.SubscriptionId, this.storageName);

            BlobHelpers.CreateInitialTablesAndBlobContainers(storageName, key);
            CacheDetails(activationInfo.HostSubscription.SubscriptionId, key, storageName, activationInfo.HostSubscription.OrganizationId);
            var orgGroups = AzureADGraphApiUtil.GetAllGroupsForOrganization(activationInfo.Organization.Id);
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
            });
        }

        private async Task SaveEnrolledSubscriptions(ActivationInfo activationInfo)
        {
            foreach (var enrolledSubscription in activationInfo.EnrolledSubscriptions)
            {
                Run.WithProgressBackOff(5, 1, 5, () =>
                {
                    AzureResourceManagerUtil.GrantRoleToServicePrincipalOnSubscription(activationInfo.HostSubscription.ServicePrincipalId, enrolledSubscription.SubscriptionId);
                });

                var subscriptionFromDb = this.coreRepository.GetSubscription(enrolledSubscription.SubscriptionId);
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
                await this.coreRepository.SaveSubscription(subscriptionToSave);
            }
        }

        private static void CacheDetails(string subscriptionId, string storageKey, string storageName, string organizationId)
        {
            string signedInUserUniqueId = ClaimsPrincipal.Current.SignedInUserName();
            if (!string.IsNullOrEmpty(storageName) && !string.IsNullOrEmpty(storageKey))
            {
                CacheUserDetails cud = new CacheUserDetails();
                cud.SubscriptionId = subscriptionId;
                cud.StorageName = storageName;
                cud.OrganizationId = organizationId;
                cud.StorageKey = storageKey;
                MemoryCacher.Delete(signedInUserUniqueId);
                MemoryCacher.Add(signedInUserUniqueId, cud, DateTime.Now.AddMinutes(15));
            }
        }

        private static async Task AddEnrollingUserToAllGroups(Organization org)
        {
            var userId = ClaimsPrincipal.Current.UserId();
            await AzureADGraphApiUtil.AddUserToGroup(org.Id, org.CreateProductGroup, userId);
            if (org.DeployGroup != org.CreateProductGroup)
            {
                await AzureADGraphApiUtil.AddUserToGroup(org.Id, org.DeployGroup, userId);
            }
            if (org.AdminGroup != org.CreateProductGroup)
            {
                await AzureADGraphApiUtil.AddUserToGroup(org.Id, org.AdminGroup, userId);
            }
        }

        #endregion
    }
}