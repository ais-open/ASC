//----------------------------------------------------------------------------------------------
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//----------------------------------------------------------------------------------------------
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin;

namespace AzureServiceCatalog.Web.Controllers
{
    public class AccountController : Controller
    {
        private const string msDomain = ".onmicrosoft.com";

        // sign in triggered from the Sign In gesture in the UI
        // configured to return to the home page upon successful authentication
        public void SignIn(string directoryName, bool isMsa = false, bool activation = false)
        {
            // note configuration (keys, etc…) will not necessarily understand this authority.
            if (isMsa && !Request.IsAuthenticated)
            {
                if (!directoryName.Contains(".")) // no domain detected so add default
                {
                    directoryName = directoryName + msDomain;
                }

                HttpContext.GetOwinContext().Environment.Add("Authority", string.Format(ConfigurationManager.AppSettings["ida:Authority"] + "OAuth2/Authorize", directoryName));
                HttpContext.GetOwinContext().Environment.Add("DomainHint", "live.com");
            }

            var redirectUrl = activation ? this.Url.Action("Index", "Home", new { activation = true }) : this.Url.Action("Index", "Home");
            HttpContext.GetOwinContext().Authentication.Challenge(new AuthenticationProperties { RedirectUri = redirectUrl }, OpenIdConnectAuthenticationDefaults.AuthenticationType);

        }

        public void AppSourceActivation()
        {
            var redirectUrl = this.Url.Action("Index", "Home", new { activation = true });
            HttpContext.GetOwinContext().Authentication.Challenge(new AuthenticationProperties { RedirectUri = redirectUrl }, OpenIdConnectAuthenticationDefaults.AuthenticationType);
        }

        // sign out triggered from the Sign Out gesture in the UI
        // after sign out, it redirects to Post_Logout_Redirect_Uri (as set in Startup.Auth.cs)
        public void SignOut()
        {
            HttpContext.GetOwinContext().Authentication.SignOut(
                OpenIdConnectAuthenticationDefaults.AuthenticationType, CookieAuthenticationDefaults.AuthenticationType);
        }
    }
}