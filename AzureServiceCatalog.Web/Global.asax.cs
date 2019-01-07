using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Microsoft.ApplicationInsights;
using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;
using System.Configuration;
using Microsoft.ApplicationInsights.Extensibility;
using System.Security.Claims;

namespace AzureServiceCatalog.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private TelemetryClient _ai = new TelemetryClient();

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        protected void Application_Start()
        {
            TelemetryConfiguration.Active.InstrumentationKey = ConfigurationManager.AppSettings["iKey"];

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        void Application_Error(object sender, EventArgs e)
        {  
            var error = Server.GetLastError();
            if (error != null)
            {
                _ai.TrackException(error);
                Trace.TraceError("\n" + DateTime.UtcNow);
                Trace.TraceError("Stack Trace:" + error.ToString());
                Trace.TraceError("UserName :" + this.User.Identity.Name.ToString());
                Trace.TraceError("Tenant Id :" + ClaimsPrincipal.Current.Identity.Name.ToString());
            }
        }
    }
}
