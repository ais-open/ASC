using AzureServiceCatalog.Web.Models;
using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace AzureServiceCatalog.Web.Controllers
{
    public class ActivationController : ApiController
    {
        public async Task<IHttpActionResult> Post(ActivationInfo activationInfo)
        {
            try
            {
                var processor = new ActivationProcessor();
                await processor.SaveActivation(activationInfo);
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
