using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(AzureServiceCatalog.Web.Startup))]
namespace AzureServiceCatalog.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
