using AzureServiceCatalog.Web.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace AzureServiceCatalog.Web
{
    public partial class Startup
    {
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Boilerplate auth code")]
        public static void ConfigureAuth(IAppBuilder app)
        {
            string commonAuthority = string.Format(Config.Authority, "common");
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                ExpireTimeSpan = new TimeSpan(3, 0, 0),
                SlidingExpiration = true
            });

            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions
                {
                    //ProtocolValidator = new Microsoft.IdentityModel.Protocols.OpenIdConnectProtocolValidator() { RequireNonce = false },
                    ClientId = Config.ClientId,
                    Authority = commonAuthority,
                    UseTokenLifetime = false,
                    TokenValidationParameters = new System.IdentityModel.Tokens.TokenValidationParameters
                    {
                        // we inject our own multitenant validation logic
                        ValidateIssuer = false,
                    },
                    Notifications = new OpenIdConnectAuthenticationNotifications()
                    {
                        RedirectToIdentityProvider = (context) =>
                        {
                            if (IsApiRequest(context.Request))
                            {
                                context.HandleResponse();
                                context.OwinContext.Response.StatusCode = (int)System.Net.HttpStatusCode.Unauthorized;
                            }
                            else
                            {
                                // This ensures that the address used for sign in and sign out is picked up dynamically from the request
                                // this allows you to deploy your app (to Azure Web Sites, for example) without having to change settings
                                // Remember that the base URL of the address used here must be provisioned in Azure AD beforehand.
                                string appBaseUrl = context.Request.Scheme + "://" + context.Request.Host + context.Request.PathBase;

                                object obj = null;
                                if (context.OwinContext.Environment.TryGetValue("Authority", out obj))
                                {
                                    string authority = obj as string;
                                    if (authority != null)
                                    {
                                        context.ProtocolMessage.IssuerAddress = authority;
                                    }
                                }
                                if (context.OwinContext.Environment.TryGetValue("DomainHint", out obj))
                                {
                                    string domainHint = obj as string;
                                    if (domainHint != null)
                                    {
                                        context.ProtocolMessage.SetParameter("domain_hint", domainHint);
                                    }
                                }
                                if (context.OwinContext.Environment.TryGetValue("owin.RequestQueryString", out obj))
                                {
                                    var queryString = obj as string;
                                    if (queryString.StartsWith("domain="))
                                    {
                                        var domain = queryString.Substring(7).Replace(".onmicrosoft.com", null);
                                        context.ProtocolMessage.PostLogoutRedirectUri = $"{appBaseUrl}/{domain}";
                                    }
                                    else
                                    {
                                        context.ProtocolMessage.PostLogoutRedirectUri = new UrlHelper(HttpContext.Current.Request.RequestContext).Action(
                                            "Index", "Home", null, HttpContext.Current.Request.Url.Scheme
                                            );
                                    }
                                }
                                else
                                {
                                    context.ProtocolMessage.PostLogoutRedirectUri = new UrlHelper(HttpContext.Current.Request.RequestContext).Action(
                                        "Index", "Home", null, HttpContext.Current.Request.Url.Scheme
                                        );
                                }
                                context.ProtocolMessage.RedirectUri = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Path);
                                context.ProtocolMessage.Resource = Config.AzureResourceManagerIdentifier;
                            }
                            return Task.FromResult(0);
                        },
                        AuthorizationCodeReceived = (context) =>
                        {
                            ClientCredential credential = new ClientCredential(Config.ClientId, Config.Password);
                            string tenantID = context.AuthenticationTicket.Identity.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value;
                            string signedInUserUniqueName = context.AuthenticationTicket.Identity.FindFirst(ClaimTypes.Name).Value.Split('#')[context.AuthenticationTicket.Identity.FindFirst(ClaimTypes.Name).Value.Split('#').Length - 1];

                            var tokenCache = new AdalTokenCache(signedInUserUniqueName);
                            tokenCache.Clear();

                            AuthenticationContext authContext = new AuthenticationContext(string.Format(Config.Authority, tenantID), tokenCache);

                            AuthenticationResult result = authContext.AcquireTokenByAuthorizationCode(
                                context.Code, new Uri(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Path)), credential, Config.GraphAPIIdentifier);

                            return Task.FromResult(0);
                        },
                        SecurityTokenValidated = (context) =>
                        {
                            // we use this notification for injecting our custom logic
                            // retriever caller data from the incoming principal
                            string issuer = context.AuthenticationTicket.Identity.FindFirst("iss").Value;
                            if (!issuer.StartsWith("https://sts.windows.net/"))
                            {
                                // the caller is not from a trusted issuer - throw to block the authentication flow
                                throw new System.IdentityModel.Tokens.SecurityTokenValidationException();
                            }
                            return Task.FromResult(0);
                        }
                        //AuthenticationFailed = (context) =>
                        //{
                        //    context.OwinContext.Response.Redirect(new UrlHelper(HttpContext.Current.Request.RequestContext).
                        //        Action("Index", "Home", null, HttpContext.Current.Request.Url.Scheme));
                        //    context.HandleResponse(); // Suppress the exception
                        //        return Task.FromResult(0);
                        //}
                    }
                });
        }

        private static bool IsApiRequest(Microsoft.Owin.IOwinRequest request)
        {
            string apiPath = VirtualPathUtility.ToAbsolute("~/api/");
            return request.Uri.LocalPath.StartsWith(apiPath);
        }
    }
}