using AzureServiceCatalog.Web.Models;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace AzureServiceCatalog.Web.Controllers
{
    public class OrganizationController : ApiController
    {
        private TableCoreRepository coreRepository = new TableCoreRepository();

        public async Task<Organization> Get()
        {
            var tenantId = ClaimsPrincipal.Current.TenantId();
            var organization = await this.coreRepository.GetOrganization(tenantId);
            organization.OrganizationADGroups = AzureADGraphApiUtil.GetAllGroupsForOrganization(tenantId);
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