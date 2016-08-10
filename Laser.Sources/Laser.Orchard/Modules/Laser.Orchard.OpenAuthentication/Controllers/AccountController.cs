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
using System.Web.Script.Serialization;

namespace Laser.Orchard.OpenAuthentication.Controllers {
    [Themed]
    public class AccountController : Controller {
        private readonly INotifier _notifier;
        private readonly IOrchardOpenAuthWebSecurity _orchardOpenAuthWebSecurity;
        private readonly IAuthenticationService _authenticationService;
        private readonly IOpenAuthMembershipServices _openAuthMembershipServices;
        private readonly IOrchardOpenAuthClientProvider _openAuthClientProvider;
        private readonly IControllerContextAccessor _controllerContextAccessor;

        public AccountController(
            INotifier notifier,
            IOrchardOpenAuthWebSecurity orchardOpenAuthWebSecurity,
            IAuthenticationService authenticationService,
            IOpenAuthMembershipServices openAuthMembershipServices,
            IOrchardOpenAuthClientProvider openAuthClientProvider,
            IControllerContextAccessor controllerContextAccessor) {
            _notifier = notifier;
            _orchardOpenAuthWebSecurity = orchardOpenAuthWebSecurity;
            _authenticationService = authenticationService;
            _openAuthMembershipServices = openAuthMembershipServices;
            _openAuthClientProvider = openAuthClientProvider;
            _controllerContextAccessor = controllerContextAccessor;

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

        [AlwaysAccessible]
        public ActionResult ExternalLogOn(string returnUrl) {
            AuthenticationResult result = _orchardOpenAuthWebSecurity.VerifyAuthentication(Url.OpenAuthLogOn(returnUrl));

            if (!result.IsSuccessful) {
                _notifier.Error(T("Your authentication request failed."));

                return new RedirectResult(Url.LogOn(returnUrl));
            }

            if (_orchardOpenAuthWebSecurity.Login(result.Provider, result.ProviderUserId)) {
                _notifier.Information(T("You have been logged using your {0} account.", result.Provider));

                return this.RedirectLocal(returnUrl);
            }

            var authenticatedUser = _authenticationService.GetAuthenticatedUser();

            if (authenticatedUser != null) {
                // If the current user is logged in add the new account
                _orchardOpenAuthWebSecurity.CreateOrUpdateAccount(result.Provider, result.ProviderUserId,
                                                                  authenticatedUser, result.ExtraData.ToJson());

                _notifier.Information(T("Your {0} account has been attached to your local account.", result.Provider));

                return this.RedirectLocal(returnUrl);
            }

            if (_openAuthMembershipServices.CanRegister()) {
                result = _openAuthClientProvider.GetUserData(result.Provider, result, result.ExtraData["accesstoken"]);
                var createUserParams = new OpenAuthCreateUserParams(result.UserName,
                                                                    result.Provider,
                                                                    result.ProviderUserId,
                                                                    result.ExtraData);
                createUserParams = _openAuthClientProvider.NormalizeData(result.Provider, createUserParams);
                var newUser = _openAuthMembershipServices.CreateUser(createUserParams);

                _notifier.Information(
                    T("You have been logged in using your {0} account. We have created a local account for you with the name '{1}'", result.Provider, newUser.UserName));

                _orchardOpenAuthWebSecurity.CreateOrUpdateAccount(result.Provider,
                                                                  result.ProviderUserId,
                                                                  newUser,
                                                                  result.ExtraData.ToJson());

                _authenticationService.SignIn(newUser, false);

                return this.RedirectLocal(returnUrl);
            }

            string loginData = _orchardOpenAuthWebSecurity.SerializeProviderUserId(result.Provider,
                                                                                   result.ProviderUserId);

            ViewBag.ProviderDisplayName = _orchardOpenAuthWebSecurity.GetOAuthClientData(result.Provider).DisplayName;
            ViewBag.ReturnUrl = returnUrl;

            return new RedirectResult(Url.LogOn(returnUrl, result.UserName, loginData));
        }
        [OutputCache(NoStore = true, Duration = 0)]
        public JsonResult ExternalTokenLogOn(string __provider__, string token, string secret = "") {
            return ExternalTokenLogOnLogic(__provider__, token, secret);
        }

        [OutputCache(NoStore = true, Duration = 0)]
        public JsonResult ExternalTokenLogOnSsl(string __provider__, string token, string secret = "") {
            return ExternalTokenLogOnLogic(__provider__, token, secret);
        }

        [OutputCache(NoStore = true, Duration = 0)]
        public JsonResult ExternalTokenLogOnLogic(string __provider__, string token, string secret = "") {
            TempDataDictionary registeredServicesData = new TempDataDictionary();

            try {
                if (String.IsNullOrWhiteSpace(__provider__) || String.IsNullOrWhiteSpace(token)) {
                    return Json(new { success = false, registeredServices = registeredServicesData, message = T("One or more of the required parameters was not provided or was an empty string.").Text }, JsonRequestBehavior.AllowGet);
                }

                AuthenticationResult dummy = new AuthenticationResult(true);
                AuthenticationResult authResult = _openAuthClientProvider.GetUserData(__provider__, dummy, token, secret);
                
                if (!authResult.IsSuccessful) {
                    return Json(new { success = false, registeredServices = registeredServicesData, message = T("Token authentication failed.").Text }, JsonRequestBehavior.AllowGet);
                }
                else {
                    if (_orchardOpenAuthWebSecurity.Login(authResult.Provider, authResult.ProviderUserId)) {
                        if (HttpContext.Response.Cookies.Count == 0)
                            return Json(new { success = false, registeredServices = registeredServicesData, message = T("Unable to send back a cookie.").Text }, JsonRequestBehavior.AllowGet);
                        else {
                            registeredServicesData = _controllerContextAccessor.Context.Controller.TempData;
                            return Json(new { success = true, registeredServices = registeredServicesData, message = T("You have been logged using your {0} account.", authResult.Provider).Text }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    var authenticatedUser = _authenticationService.GetAuthenticatedUser();

                    if (authenticatedUser != null) {
                        // If the current user is logged in add the new account
                        _orchardOpenAuthWebSecurity.CreateOrUpdateAccount(authResult.Provider, authResult.ProviderUserId,
                                                                          authenticatedUser, authResult.ExtraData.ToJson());

                        if (HttpContext.Response.Cookies.Count == 0)
                            return Json(new { success = false, registeredServices = registeredServicesData, message = T("Unable to send back a cookie.").Text }, JsonRequestBehavior.AllowGet);
                        else {
                            registeredServicesData = _controllerContextAccessor.Context.Controller.TempData;
                            return Json(new { success = true, registeredServices = registeredServicesData, message = T("Your {0} account has been attached to your local account.", authResult.Provider).Text }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    if (_openAuthMembershipServices.CanRegister()) {
                        var createUserParams = new OpenAuthCreateUserParams(authResult.UserName,
                                                                            authResult.Provider,
                                                                            authResult.ProviderUserId,
                                                                            authResult.ExtraData);
                        createUserParams = _openAuthClientProvider.NormalizeData(authResult.Provider, createUserParams);
                        var newUser = _openAuthMembershipServices.CreateUser(createUserParams);
                        _orchardOpenAuthWebSecurity.CreateOrUpdateAccount(authResult.Provider,
                                                                          authResult.ProviderUserId,
                                                                          newUser, authResult.ExtraData.ToJson());

                        _authenticationService.SignIn(newUser, false);

                        if (HttpContext.Response.Cookies.Count == 0)
                            return Json(new { success = false, registeredServices = registeredServicesData, message = T("Unable to send back a cookie.").Text }, JsonRequestBehavior.AllowGet);
                        else {
                            registeredServicesData = _controllerContextAccessor.Context.Controller.TempData;
                            return Json(new { success = true, registeredServices = registeredServicesData, message = T("You have been logged in using your {0} account. We have created a local account for you with the name '{1}'", authResult.Provider, newUser.UserName).Text }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    return Json(new { success = false, registeredServices = registeredServicesData, message = T("Login failed.").Text }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e) {
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