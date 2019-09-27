using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using System.Diagnostics;
using AzureServiceCatalog.Models;
using AzureServiceCatalog.Helpers;
using AzureServiceCatalog.Web.Models;
using Newtonsoft.Json.Linq;
using System.Web;

namespace AzureServiceCatalog.Web.Controllers
{
    [RoutePrefix("api/identity")]
    public class IdentityController : ApiController
    {
        private TableCoreRepository coreRepository = new TableCoreRepository();
        [Route("full")]
        public async Task<IHttpActionResult> GetUserDetailsFull()
        {
            var thisOperationContext = new BaseOperationContext("IdentityController:GetUserDetailsFull")
            {
                IpAddress = HttpContext.Current.Request.UserHostAddress,
                UserId = ClaimsPrincipal.Current.SignedInUserName(),
                UserName = ClaimsPrincipal.Current.Identity.Name
            };
            try
            {
                var subscriptionInfo = new UserSubscriptionInfo();
                subscriptionInfo.UserName = ClaimsPrincipal.Current.Identity.Name;
                subscriptionInfo.FirstName = ClaimsPrincipal.Current.FirstName();
                subscriptionInfo.LastName = ClaimsPrincipal.Current.LastName();
                subscriptionInfo.DefaultAdGroup = Config.DefaultAdGroup;
                subscriptionInfo.DefaultResourceGroup = Config.DefaultResourceGroup;
                string tenantId = ClaimsPrincipal.Current.TenantId();
                string signedInUserUniqueId = ClaimsPrincipal.Current.SignedInUserName();

                var userGroupsRoles = await AzureADGraphApiHelper.GetUserGroups(signedInUserUniqueId, tenantId, thisOperationContext);
                subscriptionInfo.IsGlobalAdministrator = AzureADGraphApiHelper.IsGlobalAdministrator(userGroupsRoles, thisOperationContext);

                var org = await GetOrganization(tenantId, thisOperationContext);
                var dbOrg = await this.coreRepository.GetOrganization(tenantId, thisOperationContext);
                List<Subscription> dbSubscriptions = null;

                if (dbOrg != null)
                {
                    org.DeployGroup = dbOrg.DeployGroup;
                    org.CreateProductGroup = dbOrg.CreateProductGroup;
                    org.AdminGroup = dbOrg.AdminGroup;
                    //var userGroups = await AzureADGraphApiHelper.GetUserGroups(signedInUserUniqueId, org.Id);
                    subscriptionInfo.CanCreate = userGroupsRoles.Any(x => x.Id == dbOrg.CreateProductGroup);
                    subscriptionInfo.CanDeploy = userGroupsRoles.Any(x => x.Id == dbOrg.DeployGroup);
                    subscriptionInfo.CanAdmin = userGroupsRoles.Any(x => x.Id == dbOrg.AdminGroup);
                    dbSubscriptions = this.coreRepository.GetSubscriptionListByOrgId(tenantId, thisOperationContext);

                    if (dbSubscriptions != null && dbSubscriptions.Count > 0)
                    {
                        subscriptionInfo.IsActivatedByAdmin = (dbSubscriptions.Any(x => x.IsConnected));
                    }
                }

                subscriptionInfo.Organization = org;

                var orgGroups = await AzureADGraphApiHelper.GetAllGroupsForOrganization(org.Id, thisOperationContext);
                subscriptionInfo.OrganizationADGroups = orgGroups;
                var subscriptions = await AzureResourceManagerHelper.GetUserSubscriptions(org.Id, thisOperationContext);
                if (subscriptions != null)
                {
                    foreach (var subscription in subscriptions)
                    {
                        var userDetailVM = new UserDetailsViewModel();
                        userDetailVM.CanCreate = subscriptionInfo.CanCreate;
                        userDetailVM.CanDeploy = subscriptionInfo.CanDeploy;
                        userDetailVM.CanAdmin = subscriptionInfo.CanAdmin;
                        userDetailVM.Name = subscriptionInfo.UserName;

                        userDetailVM.IsAdminOfSubscription = await AzureResourceManagerHelper.UserCanManageAccessForSubscription(subscription.Id, thisOperationContext);
                        userDetailVM.SubscriptionName = subscription.DisplayName;
                        userDetailVM.SubscriptionId = subscription.Id;
                        userDetailVM.OrganizationId = org.Id;
                        userDetailVM.ServicePrincipalId = org.ObjectIdOfCloudSenseServicePrincipal;
                        userDetailVM.OrganizationName = org.DisplayName;

                        Subscription dbSubscription = null;
                        if (dbSubscriptions != null && dbSubscriptions.Count > 0)
                        {
                            dbSubscription = dbSubscriptions.FirstOrDefault(x => x.Id == subscription.Id) ?? null;
                        }

                        if (dbSubscription != null)
                        {
                            userDetailVM.SubscriptionIsConnected = dbSubscription.IsConnected;// true;
                            userDetailVM.IsEnrolled = dbSubscription.IsEnrolled;
                            userDetailVM.SubscriptionNeedsRepair = !await AzureResourceManagerHelper.ServicePrincipalHasReadAccessToSubscription(dbSubscription.Id, thisOperationContext);
                            if (userDetailVM.SubscriptionIsConnected)
                            {
                                string organizationId = dbSubscription.OrganizationId;
                                string storageName = dbSubscription.StorageName;
                                try
                                {
                                    string storageKey = await AzureResourceManagerHelper.GetStorageAccountKeysArm(dbSubscription.Id, dbSubscription.StorageName, thisOperationContext);
                                    CacheDetails(userDetailVM, storageKey, storageName, organizationId, signedInUserUniqueId, thisOperationContext);
                                }
                                catch (Exception ex)
                                {
                                    Trace.TraceError(ex.Message);
                                    Trace.TraceError($"Storage account: {storageName} was not found!");
                                    userDetailVM.SubscriptionIsConnected = false;
                                    userDetailVM.IsEnrolled = false;
                                }

                            }
                        }
                        subscriptionInfo.Subscriptions.Add(userDetailVM);
                    }
                }
                return this.Ok(subscriptionInfo);
            }
            catch (Exception ex)
            {
                TraceHelper.TraceError(thisOperationContext.OperationId, thisOperationContext.OperationName, ex);
                return Content(HttpStatusCode.InternalServerError, JObject.FromObject(ErrorInformation.GetInternalServerErrorInformation(thisOperationContext.OperationId, thisOperationContext.Timestamp)));
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        [Route("")]
        public async Task<IHttpActionResult> GetUserDetails()
        {
            var thisOperationContext = new BaseOperationContext("IdentityController:GetUserDetails")
            {
                IpAddress = HttpContext.Current.Request.UserHostAddress,
                UserId = ClaimsPrincipal.Current.SignedInUserName(),
                UserName = ClaimsPrincipal.Current.Identity.Name
            };
            try
            {
                var subscriptionInfo = new UserSubscriptionInfo();
                subscriptionInfo.UserName = ClaimsPrincipal.Current.Identity.Name;
                var tenantId = ClaimsPrincipal.Current.TenantId();
                var signedInUserUniqueId = ClaimsPrincipal.Current.SignedInUserName();

                var org = await this.coreRepository.GetOrganization(tenantId, thisOperationContext);
                subscriptionInfo.Organization = org;

                var userGroups = await AzureADGraphApiHelper.GetUserGroups(signedInUserUniqueId, org.Id, thisOperationContext);
                subscriptionInfo.CanCreate = userGroups.Any(x => x.Id == org.CreateProductGroup);
                subscriptionInfo.CanDeploy = userGroups.Any(x => x.Id == org.DeployGroup);
                subscriptionInfo.CanAdmin = userGroups.Any(x => x.Id == org.AdminGroup);

                return this.Ok(subscriptionInfo);
            }
            catch (Exception ex)
            {
                TraceHelper.TraceError(thisOperationContext.OperationId, thisOperationContext.OperationName, ex);
                return Content(HttpStatusCode.InternalServerError, JObject.FromObject(ErrorInformation.GetInternalServerErrorInformation(thisOperationContext.OperationId, thisOperationContext.Timestamp)));
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        [Route("organization-groups")]
        public async Task<IHttpActionResult> GetOrganizationGroups(string filter)
        {
            var thisOperationContext = new BaseOperationContext("IdentityController:GetOrganizationGroups")
            {
                IpAddress = HttpContext.Current.Request.UserHostAddress,
                UserId = ClaimsPrincipal.Current.SignedInUserName(),
                UserName = ClaimsPrincipal.Current.Identity.Name
            };
            try
            {
                string tenantId = ClaimsPrincipal.Current.TenantId();
                var orgGroups = await AzureADGraphApiHelper.GetAllGroupsForOrganization(tenantId, thisOperationContext, filter);
                return this.Ok(orgGroups);
            }
            catch (Exception ex)
            {
                TraceHelper.TraceError(thisOperationContext.OperationId, thisOperationContext.OperationName, ex);
                return Content(HttpStatusCode.InternalServerError, JObject.FromObject(ErrorInformation.GetInternalServerErrorInformation(thisOperationContext.OperationId, thisOperationContext.Timestamp)));
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        [Route("users")]
        public async Task<IHttpActionResult> GetUsers()
        {
            var thisOperationContext = new BaseOperationContext("IdentityController:GetUsers")
            {
                IpAddress = HttpContext.Current.Request.UserHostAddress,
                UserId = ClaimsPrincipal.Current.SignedInUserName(),
                UserName = ClaimsPrincipal.Current.Identity.Name
            };
            try
            {
                string tenantId = ClaimsPrincipal.Current.TenantId();
                var users = await AzureADGraphApiHelper.GetUserList(tenantId, thisOperationContext);
                return this.Ok(users);
            }
            catch (Exception ex)
            {
                TraceHelper.TraceError(thisOperationContext.OperationId, thisOperationContext.OperationName, ex);
                return Content(HttpStatusCode.InternalServerError, JObject.FromObject(ErrorInformation.GetInternalServerErrorInformation(thisOperationContext.OperationId, thisOperationContext.Timestamp)));
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        [AllowAnonymous]
        [Route("portal-url")]
        public IHttpActionResult GetPortalUrl(string extensionForUrl, string extensionType)
        {
            var thisOperationContext = new BaseOperationContext("IdentityController:GetPortalUrl")
            {
                IpAddress = HttpContext.Current.Request.UserHostAddress,
                UserId = ClaimsPrincipal.Current.SignedInUserName(),
                UserName = ClaimsPrincipal.Current.Identity.Name
            };
            try
            {
                string portalUrl = Config.PortalUrl;
                if (extensionType == "Common")
                {
                    extensionForUrl = "#" + extensionForUrl;
                }
                var updatedUrl = portalUrl + extensionForUrl;
                return this.Ok(updatedUrl);
            }
            catch (Exception ex)
            {
                TraceHelper.TraceError(thisOperationContext.OperationId, thisOperationContext.OperationName, ex);
                return Content(HttpStatusCode.InternalServerError, JObject.FromObject(ErrorInformation.GetInternalServerErrorInformation(thisOperationContext.OperationId, thisOperationContext.Timestamp)));
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        [Route("asc-contributor")]
        public async Task<IHttpActionResult> PutAscContributorRole(string subscriptionId)
        {
            var thisOperationContext = new BaseOperationContext("IdentityController:PutAscContributorRole")
            {
                IpAddress = HttpContext.Current.Request.UserHostAddress,
                UserId = ClaimsPrincipal.Current.SignedInUserName(),
                UserName = ClaimsPrincipal.Current.Identity.Name
            };
            try
            {
                var rbacClient = new RbacHelper();
                //var json = await rbacClient.CreateAscContributorRoleOnSubscription(subscriptionId);

                //dynamic role = await rbacClient.GetAscContributorRole(subscriptionId);
                //var json = await rbacClient.DeleteCustomRoleOnSubscription(subscriptionId, (string)role.name);
                var json = await rbacClient.GetRoleAssignmentsForAscContributor(subscriptionId, thisOperationContext);

                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                response.Content = json.ToStringContent();
                return this.Ok(response);
            }
            catch (Exception ex)
            {
                TraceHelper.TraceError(thisOperationContext.OperationId, thisOperationContext.OperationName, ex);
                return Content(HttpStatusCode.InternalServerError, JObject.FromObject(ErrorInformation.GetInternalServerErrorInformation(thisOperationContext.OperationId, thisOperationContext.Timestamp)));
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        #region Private Methods

        private static async Task<Organization> GetOrganization(string orgId, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "IdentityController:GetOrganization");
            try
            {
                var organization = await AzureADGraphApiHelper.GetOrganizationDetails(orgId, thisOperationContext);
                var spid = await AzureADGraphApiHelper.GetObjectIdOfServicePrincipalInOrganization(orgId, Config.ClientId, thisOperationContext);
                organization.ObjectIdOfCloudSenseServicePrincipal = spid;
                return organization;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        private static void CacheDetails(UserDetailsViewModel userDetailViewModel, string storageKey, string storageName, string organizationId, string signedInUserUniqueId, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "IdentityController:CacheDetails");
            try
            {
                if (!string.IsNullOrEmpty(storageName) && !string.IsNullOrEmpty(storageKey))
                {
                    CacheUserDetails cud = new CacheUserDetails();
                    cud.SubscriptionId = userDetailViewModel.SubscriptionId;
                    cud.StorageName = storageName;
                    cud.OrganizationId = organizationId;
                    cud.StorageKey = storageKey;
                    MemoryCacher.Delete(signedInUserUniqueId, parentOperationContext);
                    MemoryCacher.Add(signedInUserUniqueId, cud, DateTime.Now.AddMinutes(15), parentOperationContext);
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