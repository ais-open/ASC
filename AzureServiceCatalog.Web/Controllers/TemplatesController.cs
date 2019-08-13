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

namespace AzureServiceCatalog.Web.Controllers
{
    public class TemplatesController : ApiController
    {
        private TableRepository repository = new TableRepository();

        public async Task<IHttpActionResult> Get()
        {
            try
            {
                var list = await repository.GetTemplates();
                return this.Ok(list);
            }
            catch (Exception)
            {
                return Content(HttpStatusCode.InternalServerError, JObject.FromObject(ErrorInformation.GetInternalServerErrorInformation()));
            }
            finally
            {

            }
        }

        public async Task<IHttpActionResult> Get( string id)
        {
            try
            {
                var item = await repository.GetTemplate(id);
                return this.Ok(item);
            }
            catch (Exception)
            {
                return Content(HttpStatusCode.InternalServerError, JObject.FromObject(ErrorInformation.GetInternalServerErrorInformation()));
            }
            finally
            {

            }
        }

        [ADGroupAuthorize(SecurityGroupType.CanCreate)]
        public async Task<IHttpActionResult> Post(TemplateViewModel template)
        {
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
                    TemplateViewModel savedTemplateEntity = await repository.SaveTemplate(template);
                    return this.Ok(savedTemplateEntity);
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

        [ADGroupAuthorize(SecurityGroupType.CanCreate)]
        public async Task<IHttpActionResult> Delete(string name)
        {
            try
            {
                await repository.DeleteTemplate(name);
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

        [HttpGet]
        [Route("api/templates/{id}/download")]
        public async Task<IHttpActionResult> Download(string id)
        {
            try
            {
                var item = await repository.GetTemplate(id);
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
