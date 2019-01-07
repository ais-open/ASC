using System;
using System.Web;
using System.Web.Mvc;
using Microsoft.ApplicationInsights;

namespace AzureServiceCatalog.Web
{
    public static class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            //filters.Add(new HandleErrorAttribute());
            filters.Add(new AIHandleErrorAttribute());
            filters.Add(new RequireHttpsAttribute());
        }
    }
}
