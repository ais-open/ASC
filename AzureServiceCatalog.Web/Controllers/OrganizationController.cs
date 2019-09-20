using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using AzureServiceCatalog.Models;
using AzureServiceCatalog.Helpers;
using AzureServiceCatalog.Web.Models;
using Newtonsoft.Json.Linq;
using System.Web;

namespace AzureServiceCatalog.Web.Controllers
{
    public class OrganizationController : ApiController
    {
        private TableCoreRepository coreRepository = new TableCoreRepository();

        public async Task<IHttpActionResult> Get()
        {
            var thisOperationContext = new BaseOperationContext("OrganizationController:Get")
            {
                IpAddress = HttpContext.Current.Request.UserHostAddress,
                UserId = ClaimsPrincipal.Current.SignedInUserName(),
                UserName = ClaimsPrincipal.Current.Identity.Name
            };
            try
            {
                var tenantId = ClaimsPrincipal.Current.TenantId();
                var organization = await this.coreRepository.GetOrganization(tenantId, thisOperationContext);
                organization.OrganizationADGroups = await AzureADGraphApiHelper.GetAllGroupsForOrganization(tenantId, thisOperationContext);
                if (organization.AdminGroupName == null || organization.CreateProductGroupName == null)
                {
                    organization.AdminGroupName = organization.OrganizationADGroups.Where(x => x.Id == organization.AdminGroup).SingleOrDefault()?.Name;
                    organization.CreateProductGroupName = organization.OrganizationADGroups.Where(x => x.Id == organization.CreateProductGroup).SingleOrDefault()?.Name;
                }
                return this.Ok(organization);
            }
            catch (Exception ex)
            {
                //Trace.TraceError(ex.Message);
                //Trace.TraceError($"AdminGroupName or CreateProductGroupName not found in the organisation : { organization.Id}");
                TraceHelper.TraceError(thisOperationContext.OperationId, thisOperationContext.OperationName, ex);
                return Content(HttpStatusCode.InternalServerError, JObject.FromObject(ErrorInformation.GetInternalServerErrorInformation()));
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        [AllowAnonymous]
        public IHttpActionResult GetByVerifiedDomain(string domain)
        {
            var thisOperationContext = new BaseOperationContext("OrganizationController:GetByVerifiedDomain")
            {
                IpAddress = HttpContext.Current.Request.UserHostAddress
            };
            try
            {
                var organization = this.coreRepository.GetOrganizationByDomain(domain.ToLower(), thisOperationContext);
                return this.Ok(organization);
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

        [ADGroupAuthorize(SecurityGroupType.CanAdmin)]
        public async Task<IHttpActionResult> Post(Organization organization)
        {
            var thisOperationContext = new BaseOperationContext("OrganizationController:Post")
            {
                IpAddress = HttpContext.Current.Request.UserHostAddress,
                UserId = ClaimsPrincipal.Current.SignedInUserName(),
                UserName = ClaimsPrincipal.Current.Identity.Name
            };
            try
            {
                if (organization == null)
                {
                    ErrorInformation errorInformation = new ErrorInformation();
                    errorInformation.Code = "InvalidRequest";
                    errorInformation.Message = "Request body is invalid.";
                    return Content(HttpStatusCode.BadRequest, JObject.FromObject(errorInformation));
                } else
                {
                    await this.coreRepository.SaveOrganization(organization, thisOperationContext);
                    return this.Ok(organization);
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

        public async Task<IHttpActionResult> Delete()
        {
            var thisOperationContext = new BaseOperationContext("OrganizationController:Delete")
            {
                IpAddress = HttpContext.Current.Request.UserHostAddress,
                UserId = ClaimsPrincipal.Current.SignedInUserName(),
                UserName = ClaimsPrincipal.Current.Identity.Name
            };
            try
            {
                var tenantId = ClaimsPrincipal.Current.TenantId();
                var organization = await this.coreRepository.GetOrganization(tenantId, thisOperationContext);
                await this.coreRepository.DeleteOrganization(organization, thisOperationContext);
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