using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace AzureServiceCatalog.Web.Models
{
    public static class ArmHttpClient
    {
        public static async Task<string> Get(string requestUrl)
        {
            return await HttpSend(HttpMethod.Get, requestUrl);
        }

        public static async Task<string> Post(string requestUrl, object body)
        {
            return await HttpSend(HttpMethod.Post, requestUrl, body);
        }
        public static async Task<string> Put(string requestUrl, object body)
        {
            return await HttpSend(HttpMethod.Put, requestUrl, body);
        }
        public static async Task<string> Delete(string requestUrl)
        {
            return await HttpSend(HttpMethod.Delete, requestUrl);
        }

        #region Private Methods

        private static async Task<string> HttpSend(HttpMethod method, string requestUrl, object body =  null)
        {
            //TODO: need to determine if any calls need User authentication
            var httpClient = Utils.GetAuthenticatedHttpClientForApp();
            var request = new HttpRequestMessage(method, requestUrl);
            request.Headers.Add(Utils.MSClientRequestHeader, Config.AscAppId);
            request.Headers.Add(HttpRequestHeader.Accept.ToString(), "application/json");
            request.Headers.Add(HttpRequestHeader.ContentType.ToString(), "application/json");
            if (body != null)
            {
                var json = body as string;
                if (json == null)
                {
                    request.Content = JsonConvert.SerializeObject(body).ToStringContent();
                }
                else
                {
                    request.Content = json.ToStringContent();
                }
            }
            HttpResponseMessage response = await httpClient.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();
            return result;
        }

        #endregion
    }
}