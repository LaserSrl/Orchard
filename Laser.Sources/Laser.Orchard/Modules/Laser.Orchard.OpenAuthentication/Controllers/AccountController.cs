using DotNetOpenAuth.AspNet;
using Laser.Orchard.OpenAuthentication.Extensions;
using Laser.Orchard.OpenAuthentication.Security;
using Laser.Orchard.OpenAuthentication.Services;
using Laser.Orchard.OpenAuthentication.Services.Clients;
using Laser.Orchard.StartupConfig.Services;
using Orchard;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc.Extensions;
using Orchard.Security;
using Orchard.Themes;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Linq;
using Laser.Orchard.OpenAuthentication.Models;

namespace Laser.Orchard.OpenAuthentication.Controllers {
    [Themed]
    public class AccountController : Controller {
        private readonly INotifier _notifier;
        private readonly IOrchardOpenAuthWebSecurity _orchardOpenAuthWebSecurity;
        private readonly IAuthenticationService _authenticationService;
        private readonly IOpenAuthMembershipServices _openAuthMembershipServices;
        private readonly IOrchardOpenAuthClientProvider _openAuthClientProvider;
        private readonly IControllerContextAccessor _controllerContextAccessor;
        private readonly IEnumerable<IExternalAuthenticationClient> _openAuthAuthenticationClients;

        public AccountController(
            INotifier notifier,
            IOrchardOpenAuthWebSecurity orchardOpenAuthWebSecurity,
            IAuthenticationService authenticationService,
            IOpenAuthMembershipServices openAuthMembershipServices,
            IOrchardOpenAuthClientProvider openAuthClientProvider,
            IControllerContextAccessor controllerContextAccessor,
            IEnumerable<IExternalAuthenticationClient> openAuthAuthenticationClients) {
            _notifier = notifier;
            _orchardOpenAuthWebSecurity = orchardOpenAuthWebSecurity;
            _authenticationService = authenticationService;
            _openAuthMembershipServices = openAuthMembershipServices;
            _openAuthClientProvider = openAuthClientProvider;
            _controllerContextAccessor = controllerContextAccessor;
            _openAuthAuthenticationClients = openAuthAuthenticationClients;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        [HttpPost]
        [AlwaysAccessible]
        public ActionResult ExternalLogOn(string providerName, string returnUrl) {
            return new OpenAuthLoginResult(providerName, Url.OpenAuthLogOn(returnUrl));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [AlwaysAccessible]
        public ActionResult ExternalLogOn(string returnUrl) {
            AuthenticationResult result = _orchardOpenAuthWebSecurity.VerifyAuthentication(Url.OpenAuthLogOn(returnUrl));

            //LV
            var client = _openAuthAuthenticationClients
                .SingleOrDefault(o => o.ProviderName.Equals(result.Provider, StringComparison.OrdinalIgnoreCase));

            if (!result.IsSuccessful) {
                _notifier.Error(T("Your authentication request failed."));

                return new RedirectResult(Url.LogOn(returnUrl));
            }


            if (_orchardOpenAuthWebSecurity.Login(result.Provider, result.ProviderUserId)) {
                _notifier.Information(T("You have been logged using your {0} account.", result.Provider));
                return this.RedirectLocal(returnUrl);
            }

            var authenticatedUser = _authenticationService.GetAuthenticatedUser();

            UserAccountLogin newAuthenticatedUser = new UserAccountLogin();
            if (authenticatedUser != null) {
                newAuthenticatedUser.Email = authenticatedUser.Email;
                newAuthenticatedUser.UserName = authenticatedUser.UserName;
            }

            if (authenticatedUser != null) {
                // If the current user is logged in add the new account
                _orchardOpenAuthWebSecurity.CreateOrUpdateAccount(result.Provider, result.ProviderUserId,
                                                                  newAuthenticatedUser);

                _notifier.Information(T("Your {0} account has been attached to your local account.", result.Provider));

                return this.RedirectLocal(returnUrl);
            }

            if (_openAuthMembershipServices.CanRegister()) {
                
                // LV Normalize data
                string UserName=string.Empty;
                if (result.Provider == "twitter")
                    UserName = result.UserName.Trim();
                else
                    UserName = result.UserName;


                OpenAuthCreateUserParams normalize = client.NormalizeData(new OpenAuthCreateUserParams(UserName,
                                                                                                        result.Provider,
                                                                                                        result.ProviderUserId,
                                                                                                        result.ExtraData));

                var newUser =
                    _openAuthMembershipServices.CreateUser(new OpenAuthCreateUserParams(normalize.UserName,
                                                                                        result.Provider,
                                                                                        result.ProviderUserId,
                                                                                        normalize.ExtraData));

                _notifier.Information(
                    T("You have been logged in using your {0} account. We have created a local account for you with the name '{1}'", result.Provider, newUser.UserNameOrchard));

                _orchardOpenAuthWebSecurity.CreateOrUpdateAccount(result.Provider,
                                                                  result.ProviderUserId,
                                                                  newUser);
                
                IUser newUserI = newUser.IUserParz;
                _authenticationService.SignIn(newUserI, false);

                return this.RedirectLocal(returnUrl);
            }

            string loginData = _orchardOpenAuthWebSecurity.SerializeProviderUserId(result.Provider,
                                                                                   result.ProviderUserId);

            ViewBag.ProviderDisplayName = _orchardOpenAuthWebSecurity.GetOAuthClientData(result.Provider).DisplayName;
            ViewBag.ReturnUrl = returnUrl;

            return new RedirectResult(Url.LogOn(returnUrl, result.UserName, loginData));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="__provider__"></param>
        /// <param name="token"></param>
        /// <param name="secret"></param>
        /// <returns></returns>
        [OutputCache(NoStore = true, Duration = 0)]
        public JsonResult ExternalTokenLogOn(string __provider__, string token, string secret = "") {
            return ExternalTokenLogOnLogic(__provider__, token, secret);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="__provider__"></param>
        /// <param name="token"></param>
        /// <param name="secret"></param>
        /// <returns></returns>
        [OutputCache(NoStore = true, Duration = 0)]
        public JsonResult ExternalTokenLogOnSsl(string __provider__, string token, string secret = "") {
            return ExternalTokenLogOnLogic(__provider__, token, secret);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="__provider__"></param>
        /// <param name="token"></param>
        /// <param name="secret"></param>
        /// <returns></returns>
        [OutputCache(NoStore = true, Duration = 0)]
        public JsonResult ExternalTokenLogOnLogic(string __provider__, string token, string secret = "") {
            TempDataDictionary registeredServicesData = new TempDataDictionary();

            //LV
            var client = _openAuthAuthenticationClients
                .SingleOrDefault(o => o.ProviderName.Equals(__provider__, StringComparison.OrdinalIgnoreCase));

            try {
                if (String.IsNullOrWhiteSpace(__provider__) || String.IsNullOrWhiteSpace(token)) {
                    return Json(new { success = false, registeredServices = registeredServicesData, message = T("One or more of the required parameters was not provided or was an empty string.").Text }, JsonRequestBehavior.AllowGet);
                }

                AuthenticationResult authResult = _openAuthClientProvider.GetUserData(__provider__, token, secret);

                if (!authResult.IsSuccessful) {
                    return Json(new { success = false, registeredServices = registeredServicesData, message = T("Token authentication failed.").Text }, JsonRequestBehavior.AllowGet);
                } 
                else 
                {
                    if (_orchardOpenAuthWebSecurity.Login(authResult.Provider, authResult.ProviderUserId)) {
                        if (HttpContext.Response.Cookies.Count == 0)
                            return Json(new { success = false, registeredServices = registeredServicesData, message = T("Unable to send back a cookie.").Text }, JsonRequestBehavior.AllowGet);
                        else {
                            registeredServicesData = _controllerContextAccessor.Context.Controller.TempData;
                            return Json(new { success = true, registeredServices = registeredServicesData, message = T("You have been logged using your {0} account.", authResult.Provider).Text }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    var authenticatedUser = _authenticationService.GetAuthenticatedUser();


                    UserAccountLogin newAuthenticatedUser = new UserAccountLogin(); 
                    if (authenticatedUser != null)
                    {
                        newAuthenticatedUser.Email=authenticatedUser.Email;
                        newAuthenticatedUser.UserName = authenticatedUser.UserName;
                    }


                    if (authenticatedUser != null) {
                        // If the current user is logged in add the new account
                        _orchardOpenAuthWebSecurity.CreateOrUpdateAccount(authResult.Provider, authResult.ProviderUserId,
                                                                          newAuthenticatedUser);

                        if (HttpContext.Response.Cookies.Count == 0)
                            return Json(new { success = false, registeredServices = registeredServicesData, message = T("Unable to send back a cookie.").Text }, JsonRequestBehavior.AllowGet);
                        else {
                            registeredServicesData = _controllerContextAccessor.Context.Controller.TempData;
                            return Json(new { success = true, registeredServices = registeredServicesData, message = T("Your {0} account has been attached to your local account.", authResult.Provider).Text }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    if (_openAuthMembershipServices.CanRegister()) {

                        // LV Normalize data
                        string UserName = string.Empty;
                        if (__provider__ == "twitter")
                            UserName = authResult.UserName.Trim();
                        else
                            UserName = authResult.UserName;


                        OpenAuthCreateUserParams normalize = client.NormalizeData(new OpenAuthCreateUserParams(UserName,
                                                                                                                authResult.Provider,
                                                                                                                authResult.ProviderUserId,
                                                                                                                authResult.ExtraData));

                        var newUser =
                            _openAuthMembershipServices.CreateUser(new OpenAuthCreateUserParams(normalize.UserName,
                                                                                                authResult.Provider,
                                                                                                authResult.ProviderUserId,
                                                                                                authResult.ExtraData));
                        _orchardOpenAuthWebSecurity.CreateOrUpdateAccount(authResult.Provider,
                                                                          authResult.ProviderUserId,
                                                                          newUser);

                        IUser newUserI = newUser.IUserParz;
                        _authenticationService.SignIn(newUserI, false);

                        if (HttpContext.Response.Cookies.Count == 0)
                            return Json(new { success = false, registeredServices = registeredServicesData, message = T("Unable to send back a cookie.").Text }, JsonRequestBehavior.AllowGet);
                        else {
                            registeredServicesData = _controllerContextAccessor.Context.Controller.TempData;
                            return Json(new { success = true, registeredServices = registeredServicesData, message = T("You have been logged in using your {0} account. We have created a local account for you with the name '{1}'", authResult.Provider, newUser.UserName).Text }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    return Json(new { success = false, registeredServices = registeredServicesData, message = T("Login failed.").Text }, JsonRequestBehavior.AllowGet);
                }
            } catch (Exception e) {
                return Json(new { success = false, registeredServices = registeredServicesData, message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }

    internal class OpenAuthLoginResult : ActionResult {
        private readonly string _providerName;
        private readonly string _returnUrl;

        public OpenAuthLoginResult(string providerName, string returnUrl) {
            _providerName = providerName;
            _returnUrl = returnUrl;
        }

        public override void ExecuteResult(ControllerContext context) {
            using (new TransactionScope(TransactionScopeOption.Suppress)) {
                var httpContext = HttpContext.Current;
                var securityManagerWrapper = httpContext.Request.RequestContext.GetWorkContext().Resolve<IOpenAuthSecurityManagerWrapper>();
                securityManagerWrapper.RequestAuthentication(_providerName, _returnUrl);
            }
        }
    }
}