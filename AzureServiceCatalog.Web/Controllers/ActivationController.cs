using System;
using System.Threading.Tasks;
using System.Web.Http;
using AzureServiceCatalog.Models;
using AzureServiceCatalog.Helpers;
using AzureServiceCatalog.Web.Models;
using Newtonsoft.Json.Linq;
using System.Net;

namespace AzureServiceCatalog.Web.Controllers
{
    public class ActivationController : ApiController
    {
        public async Task<IHttpActionResult> Post(ActivationInfo activationInfo)
        {
            try
            {
                if (activationInfo == null)
                {
                    ErrorInformation errorInformation = new ErrorInformation();
                    errorInformation.Code = "InvalidRequest";
                    errorInformation.Message = "Request body is invalid.";
                    return Content(HttpStatusCode.BadRequest, JObject.FromObject(errorInformation));
                } else
                {
                    var activationHelper = new ActivationHelper();
                    await activationHelper.SaveActivation(activationInfo);
                    return this.Ok();
                }
            }
            catch (UnauthorizedAccessException authEx)
            {
                return InternalServerError(authEx);
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
