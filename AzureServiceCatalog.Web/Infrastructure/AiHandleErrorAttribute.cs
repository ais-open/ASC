using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.ApplicationInsights;
using System.Diagnostics;

namespace AzureServiceCatalog.Web
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public sealed class AIHandleErrorAttribute : HandleErrorAttribute
    {
        private TelemetryClient _ai = new TelemetryClient();
        public override void OnException(ExceptionContext filterContext)
        {
            if (filterContext != null && filterContext.HttpContext != null && filterContext.Exception != null)
            {
                if (filterContext.HttpContext.IsCustomErrorEnabled)
                {
                    _ai.TrackException(filterContext.Exception);
                    Trace.TraceError(filterContext.Exception.ToString());
                }
            }
            base.OnException(filterContext);
        }
    }
}