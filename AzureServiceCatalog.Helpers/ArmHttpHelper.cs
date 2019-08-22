using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using AzureServiceCatalog.Models;

namespace AzureServiceCatalog.Helpers
{
    public static class ArmHttpHelper
    {
        public static async Task<string> Get(string requestUrl, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "ArmHttpHelper:Get");
            try
            {
                return await HttpSend(HttpMethod.Get, requestUrl, thisOperationContext);
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public static async Task<string> Post(string requestUrl, object body, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "ArmHttpHelper:Post");
            try
            {
                return await HttpSend(HttpMethod.Post, requestUrl, thisOperationContext, body);
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public static async Task<string> Put(string requestUrl, object body, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "ArmHttpHelper:Put");
            try
            {
                return await HttpSend(HttpMethod.Put, requestUrl, thisOperationContext, body);
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public static async Task<string> Delete(string requestUrl, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "ArmHttpHelper:Delete");
            try
            {
                return await HttpSend(HttpMethod.Delete, requestUrl, thisOperationContext);
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        #region Private Methods

        private static async Task<string> HttpSend(HttpMethod method, string requestUrl, BaseOperationContext parentOperationContext, object body =  null)
        {
            //TODO: need to determine if any calls need User authentication
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "ArmHttpHelper:HttpSend");
            try
            {
                var httpClient = Helpers.GetAuthenticatedHttpClientForApp(thisOperationContext);
                var request = new HttpRequestMessage(method, requestUrl);
                request.Headers.Add(Helpers.MSClientRequestHeader, Config.AscAppId);
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
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        #endregion
    }
}