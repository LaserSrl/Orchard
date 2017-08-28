using DotNetOpenAuth.AspNet;
using Laser.Orchard.OpenAuthentication.Extensions;
using Laser.Orchard.OpenAuthentication.Security;
using Laser.Orchard.OpenAuthentication.Services;
using Laser.Orchard.OpenAuthentication.Services.Clients;
using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.StartupConfig.ViewModels;
using Orchard;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc.Extensions;
using Orchard.Security;
using Orchard.Themes;
using Orchard.UI.Notify;
using Orchard.Users.Events;
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
        private readonly IUserEventHandler _userEventHandler;
        private readonly IOrchardOpenAuthWebSecurity _orchardOpenAuthWebSecurity;
        private readonly IAuthenticationService _authenticationService;
        private readonly IOpenAuthMembershipServices _openAuthMembershipServices;
        private readonly IOrchardOpenAuthClientProvider _openAuthClientProvider;
        private readonly IControllerContextAccessor _controllerContextAccessor;
        private readonly IUtilsServices _utilsServices;
        private readonly IOrchardServices _orchardServices;

        public AccountController(
            INotifier notifier,
            IOrchardOpenAuthWebSecurity orchardOpenAuthWebSecurity,
            IAuthenticationService authenticationService,
            IOpenAuthMembershipServices openAuthMembershipServices,
            IOrchardOpenAuthClientProvider openAuthClientProvider,
            IControllerContextAccessor controllerContextAccessor,
            IUserEventHandler userEventHandler, IUtilsServices utilsServices, IOrchardServices orchardServices) {
            _notifier = notifier;
            _orchardOpenAuthWebSecurity = orchardOpenAuthWebSecurity;
            _authenticationService = authenticationService;
            _openAuthMembershipServices = openAuthMembershipServices;
            _openAuthClientProvider = openAuthClientProvider;
            _controllerContextAccessor = controllerContextAccessor;
            _userEventHandler = userEventHandler;
            _utilsServices = utilsServices;
            _orchardServices = orchardServices;

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
                if (result.ExtraData.ContainsKey("accesstoken")) {
                    result = _openAuthClientProvider.GetUserData(result.Provider, result, result.ExtraData["accesstoken"]);
                }
                else {
                    result = _openAuthClientProvider.GetUserData(result.Provider, result, "");
                }
                var createUserParams = new OpenAuthCreateUserParams(result.UserName,
                                                                    result.Provider,
                                                                    result.ProviderUserId,
                                                                    result.ExtraData);
                createUserParams = _openAuthClientProvider.NormalizeData(result.Provider, createUserParams);
                var newUser = _openAuthMembershipServices.CreateUser(createUserParams);

                _orchardOpenAuthWebSecurity.CreateOrUpdateAccount(result.Provider,
                                                                  result.ProviderUserId,
                                                                  newUser,
                                                                  result.ExtraData.ToJson());

                _authenticationService.SignIn(newUser, false);

                if (newUser != null)
                    _notifier.Information(
                        T("You have been logged in using your {0} account. We have created a local account for you with the name '{1}'", result.Provider, newUser.UserName));
                else
                    _notifier.Error(T("Your authentication request failed."));

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
           // TempDataDictionary registeredServicesData = new TempDataDictionary();
            var result = new Response();

            try {
                if (String.IsNullOrWhiteSpace(__provider__) || String.IsNullOrWhiteSpace(token)) {
                    result = _utilsServices.GetResponse(ResponseType.None, "One or more of the required parameters was not provided or was an empty string.");
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                // ricava il return URL così come registrato nella configurazione del provider di OAuth (es. Google)
                var returnUrl = Url.MakeAbsolute(Url.Action("ExternalLogOn", "Account"));
                AuthenticationResult dummy = new AuthenticationResult(true);
                AuthenticationResult authResult = _openAuthClientProvider.GetUserData(__provider__, dummy, token, secret, returnUrl);
                IUser authenticatedUser;
                if (!authResult.IsSuccessful) {
                    result = _utilsServices.GetResponse(ResponseType.InvalidUser, "Token authentication failed.");
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else {

                    if (_orchardOpenAuthWebSecurity.Login(authResult.Provider, authResult.ProviderUserId)) {
                        if (HttpContext.Response.Cookies.Count == 0) {
                            result = _utilsServices.GetResponse(ResponseType.None, "Unable to send back a cookie.");
                            return Json(result, JsonRequestBehavior.AllowGet);
                        }                   
                        else {
                            authenticatedUser = _authenticationService.GetAuthenticatedUser();
                            _userEventHandler.LoggedIn(authenticatedUser);                          
                            return Json(GetUserResult(""), JsonRequestBehavior.AllowGet);
                        }
                    } else {
                        authenticatedUser = _authenticationService.GetAuthenticatedUser(); 
                    }

                    if (authenticatedUser != null) {
                        // If the current user is logged in add the new account
                        _orchardOpenAuthWebSecurity.CreateOrUpdateAccount(authResult.Provider, authResult.ProviderUserId,
                                                                          authenticatedUser, authResult.ExtraData.ToJson());

                        if (HttpContext.Response.Cookies.Count == 0) {
                            result = _utilsServices.GetResponse(ResponseType.None, "Unable to send back a cookie.");
                            return Json(result, JsonRequestBehavior.AllowGet);
                        }              
                        else {
                            // Handle LoggedIn Event
                            _userEventHandler.LoggedIn(authenticatedUser);
                            return Json(GetUserResult(T("Your {0} account has been attached to your local account.", authResult.Provider).Text), JsonRequestBehavior.AllowGet);
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

                        if (HttpContext.Response.Cookies.Count == 0) {
                            result = _utilsServices.GetResponse(ResponseType.None, "Unable to send back a cookie.");
                            return Json(result, JsonRequestBehavior.AllowGet);

                        }
                        else {
                            // Handle LoggedIn Event
                            _userEventHandler.LoggedIn(newUser);

                            return Json(GetUserResult(T("You have been logged in using your {0} account. We have created a local account for you with the name '{1}'", authResult.Provider, newUser.UserName).Text), JsonRequestBehavior.AllowGet);
                        }
                    }
                    result = _utilsServices.GetResponse(ResponseType.None, "Login failed.");
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e) {
                result = _utilsServices.GetResponse(ResponseType.None, e.Message);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public Response GetUserResult(String message) {
            List<string> roles = new List<string>();
            if (_orchardServices.WorkContext.CurrentUser != null) {
                roles = ((dynamic)_orchardServices.WorkContext.CurrentUser.ContentItem).UserRolesPart.Roles;
            }
            var registeredServicesData = new {
                RegisteredServices = _controllerContextAccessor.Context.Controller.TempData,
                Roles = roles
            };
            return _utilsServices.GetResponse(ResponseType.Success, message, registeredServicesData);
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