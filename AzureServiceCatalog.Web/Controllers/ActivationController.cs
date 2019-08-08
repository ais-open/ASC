using System;
using System.Threading.Tasks;
using System.Web.Http;
using AzureServiceCatalog.Models;
using AzureServiceCatalog.Helpers;

namespace AzureServiceCatalog.Web.Controllers
{
    public class ActivationController : ApiController
    {
        public async Task<IHttpActionResult> Post(ActivationInfo activationInfo)
        {
            try
            {
                var activationHelper = new ActivationHelper();
                await activationHelper.SaveActivation(activationInfo);
                return this.Ok();
            }
            catch (UnauthorizedAccessException authEx)
            {
                return InternalServerError(authEx);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
