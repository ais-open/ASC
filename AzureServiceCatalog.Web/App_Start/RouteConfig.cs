using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace AzureServiceCatalog.Web
{
    public static class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapMvcAttributeRoutes();

            routes.MapRoute(
                name: "ActivationSignIn",
                url: "account/signin",
                defaults: new { controller = "Account", action = "SignIn" }
            );

            routes.MapRoute(
                name: "AppSourceActivation",
                url: "account/appsourceactivation",
                defaults: new { controller = "Account", action = "AppSourceActivation" }
            );

            routes.MapRoute(
                name: "SignOut",
                url: "account/signout",
                defaults: new { controller = "Account", action = "SignOut" }
            );

            routes.MapRoute(
                name: "TenantSignIn",
                url: "{directoryName}/signin",
                defaults: new { controller = "Account", action = "SignIn" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{directoryName}",
                defaults: new { controller = "Home", action = "Index", directoryName = UrlParameter.Optional }
            );
        }
    }
}
