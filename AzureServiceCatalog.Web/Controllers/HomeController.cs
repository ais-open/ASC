using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Remoting;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using AzureServiceCatalog.Helpers;
using AzureServiceCatalog.Models;
using AzureServiceCatalog.Web.Models;
using Newtonsoft.Json.Linq;

namespace AzureServiceCatalog.Web.Controllers
{
    public class HomeController : Controller
    {
        private TableCoreRepository coreRepository = new TableCoreRepository();
        public ActionResult Index(string directoryName, bool activation = false, bool activationLogin = false)
        {
            var thisOperationContext = new BaseOperationContext("HomeController:Index")
            {
                IpAddress = HttpContext.Request.UserHostAddress
            };
            try
            {
                this.ViewBag.AppVersion = this.GetType().Assembly.GetName().Version.ToString();
                this.ViewBag.AppMode = ConfigurationManager.AppSettings["appMode"];
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
                        var org = this.coreRepository.GetOrganizationSync(ClaimsPrincipal.Current.TenantId(), thisOperationContext);
                        //If authenticated but not enrolled
                        if (org == null)
                        {
                            return RedirectToAction("Index", new { directoryName = directoryName, activation = true });
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
            catch (Exception ex)
            {
                TraceHelper.TraceError(thisOperationContext.OperationId, thisOperationContext.OperationName, ex);
                return View("Error");
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }
    }
}