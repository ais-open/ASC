using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.ExceptionHandling;
using Microsoft.ApplicationInsights;
using System.Diagnostics;

namespace AzureServiceCatalog.Web
{
    public class AIExceptionLogger : ExceptionLogger
    {
        private TelemetryClient _ai = new TelemetryClient();
        public override void Log(ExceptionLoggerContext context)
        {
            if (context != null && context.Exception != null)
            {
                _ai.TrackException(context.Exception);
                Trace.TraceError(context.Exception.ToString());
                Trace.TraceError("\n" + DateTime.UtcNow);
                Trace.TraceError("Url:" + context.Request.RequestUri.AbsoluteUri.ToString());
                Trace.TraceError("UserName :" + context.RequestContext.Principal.Identity.Name.ToString());
                Trace.TraceError("subscription Id" + context.Request.RequestUri.Query.Replace("?", ""));
                Trace.TraceError("Tenant Name :" + context.Request.Headers.Referrer.AbsolutePath.Replace("/", ""));
            }
            base.Log(context);
        }
    }
}