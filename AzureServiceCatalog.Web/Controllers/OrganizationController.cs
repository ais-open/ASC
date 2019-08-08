using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using AzureServiceCatalog.Models;
using AzureServiceCatalog.Helpers;

namespace AzureServiceCatalog.Web.Controllers
{
    public class OrganizationController : ApiController
    {
        private TableCoreRepository coreRepository = new TableCoreRepository();

        public async Task<Organization> Get()
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
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
                Trace.TraceError($"AdminGroupName or CreateProductGroupName not found in the organisation : { organization.Id}");
            }

            return organization;
        }

        [AllowAnonymous]
        public IHttpActionResult GetByVerifiedDomain(string domain)
        {
            var organization = this.coreRepository.GetOrganizationByDomain(domain.ToLower());
            return this.Ok(organization);
        }

        [ADGroupAuthorize(SecurityGroupType.CanAdmin)]
        public async Task<Organization> Post(Organization organization)
        {
            await this.coreRepository.SaveOrganization(organization);
            return organization;
        }

        public async Task<IHttpActionResult> Delete()
        {
            var tenantId = ClaimsPrincipal.Current.TenantId();
            var organization = await this.coreRepository.GetOrganization(tenantId);
            await this.coreRepository.DeleteOrganization(organization);
            return this.StatusCode(HttpStatusCode.NoContent);
        }
    }
}