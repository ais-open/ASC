using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using AzureServiceCatalog.Helpers;
using AzureServiceCatalog.Models;

namespace AzureServiceCatalog.Web.Controllers
{
    public class TemplatesController : ApiController
    {
        private TableRepository repository = new TableRepository();

        public async Task<IHttpActionResult> Get()
        {
            var list = await repository.GetTemplates();
            return this.Ok(list);
        }

        public async Task<IHttpActionResult> Get( string id)
        {
            var item = await repository.GetTemplate(id);
            return this.Ok(item);
        }

        [ADGroupAuthorize(SecurityGroupType.CanCreate)]
        public async Task<IHttpActionResult> Post(TemplateViewModel template)
        {
            TemplateViewModel savedTemplateEntity = await repository.SaveTemplate(template);
            return this.Ok(savedTemplateEntity);
        }

        [ADGroupAuthorize(SecurityGroupType.CanCreate)]
        public async Task<IHttpActionResult> Delete(string name)
        {
            await repository.DeleteTemplate(name);
            return this.StatusCode(HttpStatusCode.NoContent);
        }

        [HttpGet]
        [Route("api/templates/{id}/download")]
        public async Task<HttpResponseMessage> Download(string id)
        {
            var item = await repository.GetTemplate(id);
            var byteMemoryStream = new MemoryStream(Encoding.UTF8.GetBytes(item.TemplateData));
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StreamContent(byteMemoryStream);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("octet/stream");
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = string.Format("{0}.json", item.Name) //$"{item.Name}.json"
            };
            return response;
        }

    }
}
