using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using AzureServiceCatalog.Web.Models;

namespace AzureServiceCatalog.Web.Controllers
{
    public class HomeController : Controller
    {
        private TableCoreRepository coreRepository = new TableCoreRepository();
        public ActionResult Index(string directoryName, bool activation = false, bool activationLogin = false)
        {
            this.ViewBag.AppVersion = this.GetType().Assembly.GetName().Version.ToString();
            if (!activationLogin && !activation && string.IsNullOrEmpty(directoryName))
            {
                return View("Landing");
            }
            this.ViewBag.IsAuthenticated = ClaimsPrincipal.Current.Identity.IsAuthenticated.ToString().ToLower();
            this.ViewBag.Activation = activation.ToString().ToLower();
            this.ViewBag.ActivationLogin = activationLogin.ToString().ToLower();
            this.ViewBag.DirectoryName = directoryName;
            this.ViewBag.ClientId = Config.ClientId;
            if (ClaimsPrincipal.Current.Identity.IsAuthenticated)
            {
                if (!activation)
                {
                    var org = this.coreRepository.GetOrganizationSync(ClaimsPrincipal.Current.TenantId());
                    //If authenticated but not enrolled
                    if (org == null)
                    {
                        return RedirectToAction("Index", new {directoryName = directoryName, activation = true});
                    }
                    this.ViewBag.Tenant = org.VerifiedDomain;
                }
                this.ViewBag.AuthenticatedUserName = ClaimsPrincipal.Current.Identity.Name;
            }
            else if (!string.IsNullOrEmpty(directoryName))
            {
                return View("login");
            }

            return View();
        }
    }
}