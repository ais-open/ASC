using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using AzureServiceCatalog.Models;
using AzureServiceCatalog.Helpers;

namespace AzureServiceCatalog.Web
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class ADGroupAuthorizeAttribute : AuthorizeAttribute
    {
        private bool hasAuthorization = false;
        private TableCoreRepository repository = new TableCoreRepository();
        private SecurityGroupType[] allowedGroups;

        public ADGroupAuthorizeAttribute(params SecurityGroupType[] allowedGroups)
        {
            this.allowedGroups = allowedGroups;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public SecurityGroupType[] AllowedGroups
        {
            get
            {
                return this.allowedGroups;
            }
        }

        public override async Task OnAuthorizationAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            var thisOperationContext = new BaseOperationContext("ADGroupAuthorizeAttribute:OnAuthorizationAsync");
            try
            {
                var org = await this.repository.GetOrganization(ClaimsPrincipal.Current.TenantId(), thisOperationContext);
                if (org == null)
                {
                    return; // must be part of Enrollment so Organization hasn't been established yet
                }

                List<string> currentUsersGroups = await Helpers.Helpers.GetCurrentUserGroups(thisOperationContext);

                if (allowedGroups.Contains(SecurityGroupType.CanCreate) && currentUsersGroups.Contains(org.CreateProductGroup))
                {
                    hasAuthorization = true;
                }
                if (allowedGroups.Contains(SecurityGroupType.CanDepoy) && currentUsersGroups.Contains(org.DeployGroup))
                {
                    hasAuthorization = true;
                }
                if (allowedGroups.Contains(SecurityGroupType.CanAdmin) && currentUsersGroups.Contains(org.AdminGroup))
                {
                    hasAuthorization = true;
                }

                if (!hasAuthorization)
                {
                    actionContext.Response = new HttpResponseMessage(HttpStatusCode.Forbidden);
                }
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }
    }
}