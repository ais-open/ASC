using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System;
using System.Threading.Tasks;
using System.Web.Http;
using AzureServiceCatalog.Helpers;
using AzureServiceCatalog.Models;
using AzureServiceCatalog.Web.Models;
using Newtonsoft.Json.Linq;
using System.Web;
using System.Security.Claims;

namespace AzureServiceCatalog.Web.Controllers
{
    public class TemplatesController : ApiController
    {
        private TableRepository repository = new TableRepository();

        public async Task<IHttpActionResult> Get()
        {
            var thisOperationContext = new BaseOperationContext("TemplatesController:Get")
            {
                IpAddress = HttpContext.Current.Request.UserHostAddress,
                UserId = ClaimsPrincipal.Current.SignedInUserName(),
                UserName = ClaimsPrincipal.Current.Identity.Name
            };
            try
            {
                var list = await repository.GetTemplates(thisOperationContext);
                return this.Ok(list);
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

        public async Task<IHttpActionResult> Get( string id)
        {
            var thisOperationContext = new BaseOperationContext("TemplatesController:Get")
            {
                IpAddress = HttpContext.Current.Request.UserHostAddress,
                UserId = ClaimsPrincipal.Current.SignedInUserName(),
                UserName = ClaimsPrincipal.Current.Identity.Name
            };
            try
            {
                var item = await repository.GetTemplate(id, thisOperationContext);
                return this.Ok(item);
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

        [ADGroupAuthorize(SecurityGroupType.CanCreate)]
        public async Task<IHttpActionResult> Post(TemplateViewModel template)
        {
            var thisOperationContext = new BaseOperationContext("TemplatesController:Post")
            {
                IpAddress = HttpContext.Current.Request.UserHostAddress,
                UserId = ClaimsPrincipal.Current.SignedInUserName(),
                UserName = ClaimsPrincipal.Current.Identity.Name
            };
            try
            {
                if (template == null)
                {
                    ErrorInformation errorInformation = new ErrorInformation();
                    errorInformation.Code = "InvalidRequest";
                    errorInformation.Message = "Request body is invalid.";
                    //return new ActionResults.JsonActionResult(HttpStatusCode.BadRequest, JObject.FromObject(errorInformation));
                    return Content(HttpStatusCode.BadRequest, JObject.FromObject(errorInformation));
                } else
                {
                    TemplateViewModel savedTemplateEntity = await repository.SaveTemplate(template, thisOperationContext);
                    return this.Ok(savedTemplateEntity);
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

        [ADGroupAuthorize(SecurityGroupType.CanCreate)]
        public async Task<IHttpActionResult> Delete(string name)
        {
            var thisOperationContext = new BaseOperationContext("TemplatesController:Delete")
            {
                IpAddress = HttpContext.Current.Request.UserHostAddress,
                UserId = ClaimsPrincipal.Current.SignedInUserName(),
                UserName = ClaimsPrincipal.Current.Identity.Name
            };
            try
            {
                await repository.DeleteTemplate(name, thisOperationContext);
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

        [HttpGet]
        [Route("api/templates/{id}/download")]
        public async Task<IHttpActionResult> Download(string id)
        {
            var thisOperationContext = new BaseOperationContext("TemplatesController:Download")
            {
                IpAddress = HttpContext.Current.Request.UserHostAddress,
                UserId = ClaimsPrincipal.Current.SignedInUserName(),
                UserName = ClaimsPrincipal.Current.Identity.Name
            };
            try
            {
                var item = await repository.GetTemplate(id, thisOperationContext);
                var byteMemoryStream = new MemoryStream(Encoding.UTF8.GetBytes(item.TemplateData));
                HttpResponseMessage responseMsg = new HttpResponseMessage(HttpStatusCode.OK);
                responseMsg.Content = new StreamContent(byteMemoryStream);
                responseMsg.Content.Headers.ContentType = new MediaTypeHeaderValue("octet/stream");
                responseMsg.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = string.Format("{0}.json", item.Name) //$"{item.Name}.json"
                };
                IHttpActionResult response = ResponseMessage(responseMsg);
                return response;
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
