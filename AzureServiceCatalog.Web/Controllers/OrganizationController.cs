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

namespace AzureServiceCatalog.Web.Controllers
{
    public class OrganizationController : ApiController
    {
        private TableCoreRepository coreRepository = new TableCoreRepository();

        public async Task<IHttpActionResult> Get()
        {
            var tenantId = ClaimsPrincipal.Current.TenantId();
            var organization = await this.coreRepository.GetOrganization(tenantId);
            organization.OrganizationADGroups = await AzureADGraphApiUtil.GetAllGroupsForOrganization(tenantId);
            try
            {
                if (organization.AdminGroupName == null || organization.CreateProductGroupName == null)
                {
                    organization.AdminGroupName = organization.OrganizationADGroups.Where(x => x.Id == organization.AdminGroup).SingleOrDefault()?.Name;

                    organization.CreateProductGroupName = organization.OrganizationADGroups.Where(x => x.Id == organization.CreateProductGroup).SingleOrDefault()?.Name;
                }
                return this.Ok(organization);
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
                Trace.TraceError($"AdminGroupName or CreateProductGroupName not found in the organisation : { organization.Id}");
                return Content(HttpStatusCode.InternalServerError, JObject.FromObject(ErrorInformation.GetInternalServerErrorInformation()));
            }
            finally
            {

            }
        }

        [AllowAnonymous]
        public IHttpActionResult GetByVerifiedDomain(string domain)
        {
            try
            {
                var organization = this.coreRepository.GetOrganizationByDomain(domain.ToLower());
                return this.Ok(organization);
            }
            catch (Exception)
            {
                return Content(HttpStatusCode.InternalServerError, JObject.FromObject(ErrorInformation.GetInternalServerErrorInformation()));
            }
            finally
            {

            }
        }

        [ADGroupAuthorize(SecurityGroupType.CanAdmin)]
        public async Task<IHttpActionResult> Post(Organization organization)
        {
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
                    await this.coreRepository.SaveOrganization(organization);
                    return this.Ok(organization);
                }
            }
            catch (Exception)
            {
                return Content(HttpStatusCode.InternalServerError, JObject.FromObject(ErrorInformation.GetInternalServerErrorInformation()));
            }
            finally
            {

            }
        }

        public async Task<IHttpActionResult> Delete()
        {
            try
            {
                var tenantId = ClaimsPrincipal.Current.TenantId();
                var organization = await this.coreRepository.GetOrganization(tenantId);
                await this.coreRepository.DeleteOrganization(organization);
                return this.StatusCode(HttpStatusCode.NoContent);
            }
            catch (Exception)
            {
                return Content(HttpStatusCode.InternalServerError, JObject.FromObject(ErrorInformation.GetInternalServerErrorInformation()));
            }
            finally
            {

            }
        }
    }
}