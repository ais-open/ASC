using AzureServiceCatalog.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Helpers;
using Newtonsoft.Json.Linq;

namespace AzureServiceCatalog.Web.Controllers
{
    public class QuickStartTemplatesController : ApiController
    {
        public async Task<IHttpActionResult> Get()
        {
            const string memoryCacheKey = "quickStartTemplatesList";
            var cachedResponse = MemoryCacher.GetValue(memoryCacheKey);
            if (cachedResponse != null)
            {
                return Ok(cachedResponse);
            }
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("ASC");
            const string queryUrl = "https://api.github.com/search/code?q=\"azuredeploy.json\"+repo:Azure/azure-quickstart-templates+in:path&sort=indexed&per_page=100&page=1";
            // Initially set the nextLink to the origin URL
            var nextLink = new LinkItem { LinkUrl = queryUrl };
            dynamic data = null;
            var allItems = new JArray();
            while (nextLink != null)
            {
                var response = await httpClient.GetAsync(new Uri(nextLink.LinkUrl));
                var responseContent = await response.Content.ReadAsStringAsync();
                data = JObject.Parse(responseContent);
                foreach (var additionalItem in data.items)
                {
                    allItems.Add(additionalItem);
                }
                nextLink = GetNextLink(response.Headers);
            }

            data.items = allItems;
            cachedResponse = data;
            //NOTE: We are now using the github search API, which has a rate limiter of 10 requests per minute.
            //We were previously using the github repository API that had a limit of 30 requests per hour.
            MemoryCacher.Add(memoryCacheKey, cachedResponse, DateTimeOffset.Now.AddHours(1));
            return Ok(cachedResponse);
        }


        private static LinkItem GetNextLink(HttpResponseHeaders headers)
        {
            LinkItem nextLink = null;
            IEnumerable<string> values;
            if (headers.TryGetValues("Link", out values))
            {
                nextLink = LinkHeaderParser.ParseLinks(values.First())?.SingleOrDefault(l => l.Rel == "next");
            }
            return nextLink;
        }
    }
}
