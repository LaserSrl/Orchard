using DotNetOpenAuth.AspNet;
using Laser.Orchard.OpenAuthentication.Extensions;
using Laser.Orchard.OpenAuthentication.Security;
using Laser.Orchard.OpenAuthentication.Services;
using Laser.Orchard.OpenAuthentication.Services.Clients;
using Laser.Orchard.StartupConfig.IdentityProvider;
using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.StartupConfig.ViewModels;
using Newtonsoft.Json.Linq;
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
using System.Dynamic;
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
        private readonly IUtilsServices _utilsServices;
        private readonly IOrchardServices _orchardServices;
        private readonly IEnumerable<IIdentityProvider> _identityProviders;

        public AccountController(
            INotifier notifier,
            IOrchardOpenAuthWebSecurity orchardOpenAuthWebSecurity,
            IAuthenticationService authenticationService,
            IOpenAuthMembershipServices openAuthMembershipServices,
            IOrchardOpenAuthClientProvider openAuthClientProvider,
            IUserEventHandler userEventHandler,
            IUtilsServices utilsServices,
            IOrchardServices orchardServices,
            IEnumerable<IIdentityProvider> identityProviders) {
            _notifier = notifier;
            _orchardOpenAuthWebSecurity = orchardOpenAuthWebSecurity;
            _authenticationService = authenticationService;
            _openAuthMembershipServices = openAuthMembershipServices;
            _openAuthClientProvider = openAuthClientProvider;
            _userEventHandler = userEventHandler;
            _utilsServices = utilsServices;
            _orchardServices = orchardServices;
            _identityProviders = identityProviders;
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

            //Get additional UserDatas 
            if (result.ExtraData.ContainsKey("accesstoken")) {
                result = _openAuthClientProvider.GetUserData(result.Provider, result, result.ExtraData["accesstoken"]);
            }
            else {
                result = _openAuthClientProvider.GetUserData(result.Provider, result, "");
            }
            var userParams = _openAuthClientProvider.NormalizeData(result.Provider, new OpenAuthCreateUserParams(result.UserName,
                                                        result.Provider,
                                                        result.ProviderUserId,
                                                        result.ExtraData)); 
            var temporaryUser = _openAuthMembershipServices.CreateTemporaryUser(userParams);

            var masterUser = _authenticationService.GetAuthenticatedUser() ?? _orchardOpenAuthWebSecurity.GetClosestMergeableKnownUser(temporaryUser); // The autheticated User or depending from settings the first created user with the same e-mail

            if (masterUser != null) {
                // If the current user is logged in or settings ask for a user merge and we found a User with the same email creates or merge accounts
                _orchardOpenAuthWebSecurity.CreateOrUpdateAccount(result.Provider, result.ProviderUserId,
                                                                  masterUser, result.ExtraData.ToJson());

                _notifier.Information(T("Your {0} account has been attached to your local account.", result.Provider));

                if (_authenticationService.GetAuthenticatedUser() != null) { // if the user was already logged in 
                    return this.RedirectLocal(returnUrl);
                }
            }

            if (_openAuthMembershipServices.CanRegister() && masterUser == null) { // User can register and there is not a user with the same email
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

                if (newUser != null) {
                    _notifier.Information(
                        T("You have been logged in using your {0} account. We have created a local account for you with the name '{1}'", result.Provider, newUser.UserName));
                    _userEventHandler.LoggedIn(newUser);
                }
                else
                    _notifier.Error(T("Your authentication request failed."));

                return this.RedirectLocal(returnUrl);
            }
            else if (masterUser != null) {
                _authenticationService.SignIn(masterUser, false);
                _userEventHandler.LoggedIn(masterUser);
                _notifier.Information(T("You have been logged in using your {0} account.", result.Provider));
                return this.RedirectLocal(returnUrl);
            }

            // We are in the case which we cannot creates new accounts, we have no user to merge, the user is not logged in
            // so we ask to user to login to merge accounts
            string loginData = _orchardOpenAuthWebSecurity.SerializeProviderUserId(result.Provider,
                                                                                       result.ProviderUserId);

            ViewBag.ProviderDisplayName = _orchardOpenAuthWebSecurity.GetOAuthClientData(result.Provider).DisplayName;
            ViewBag.ReturnUrl = returnUrl;

            return new RedirectResult(Url.LogOn(returnUrl, result.UserName, loginData));
        }
        [OutputCache(NoStore = true, Duration = 0)]
        public ContentResult ExternalTokenLogOn(string __provider__, string token, string secret = "") {
            return ExternalTokenLogOnLogic(__provider__, token, secret);
        }

        [OutputCache(NoStore = true, Duration = 0)]
        public ContentResult ExternalTokenLogOnSsl(string __provider__, string token, string secret = "") {
            return ExternalTokenLogOnLogic(__provider__, token, secret);
        }

        [OutputCache(NoStore = true, Duration = 0)]
        private ContentResult ExternalTokenLogOnLogic(string __provider__, string token, string secret = "") {
            // TempDataDictionary registeredServicesData = new TempDataDictionary();
            var result = new Response();

            try {
                if (String.IsNullOrWhiteSpace(__provider__) || String.IsNullOrWhiteSpace(token)) {
                    result = _utilsServices.GetResponse(ResponseType.None, "One or more of the required parameters was not provided or was an empty string.");
                    return _utilsServices.ConvertToJsonResult(result);
                }

                // ricava il return URL così come registrato nella configurazione del provider di OAuth (es. Google)
                var returnUrl = Url.MakeAbsolute(Url.Action("ExternalLogOn", "Account"));
                AuthenticationResult dummy = new AuthenticationResult(true);
                AuthenticationResult authResult = _openAuthClientProvider.GetUserData(__provider__, dummy, token, secret, returnUrl);
                IUser authenticatedUser, masterUser;
                if (!authResult.IsSuccessful) {
                    result = _utilsServices.GetResponse(ResponseType.InvalidUser, "Token authentication failed.");
                    return _utilsServices.ConvertToJsonResult(result);
                }
                else {

                    if (_orchardOpenAuthWebSecurity.Login(authResult.Provider, authResult.ProviderUserId)) {
                        if (HttpContext.Response.Cookies.Count == 0) {
                            result = _utilsServices.GetResponse(ResponseType.None, "Unable to send back a cookie.");
                            return _utilsServices.ConvertToJsonResult(result);
                        }
                        else {
                            authenticatedUser = _authenticationService.GetAuthenticatedUser();
                            _userEventHandler.LoggedIn(authenticatedUser);
                            return _utilsServices.ConvertToJsonResult(_utilsServices.GetUserResponse("", _identityProviders));
                        }
                    }
                    else {
                        var userParams = _openAuthClientProvider.NormalizeData(authResult.Provider, new OpenAuthCreateUserParams(authResult.UserName,
                                                                    authResult.Provider,
                                                                    authResult.ProviderUserId,
                                                                    authResult.ExtraData));
                        var temporaryUser = _openAuthMembershipServices.CreateTemporaryUser(userParams);
                        masterUser = _authenticationService.GetAuthenticatedUser() ?? _orchardOpenAuthWebSecurity.GetClosestMergeableKnownUser(temporaryUser); // The autheticated User or depending from settings the first created user with the same e-mail
                        authenticatedUser = _authenticationService.GetAuthenticatedUser();
                    }

                    if (masterUser != null) {
                        // If the current user is logged in add the new account
                        _orchardOpenAuthWebSecurity.CreateOrUpdateAccount(authResult.Provider, authResult.ProviderUserId,
                                                                          masterUser, authResult.ExtraData.ToJson());

                        if (HttpContext.Response.Cookies.Count == 0) {
                            result = _utilsServices.GetResponse(ResponseType.None, "Unable to send back a cookie.");
                            return _utilsServices.ConvertToJsonResult(result);
                        }
                        else {
                            // Handle LoggedIn Event
                            if (authenticatedUser == null) {
                                _authenticationService.SignIn(masterUser, false);
                            }
                            _userEventHandler.LoggedIn(masterUser);
                            return _utilsServices.ConvertToJsonResult(_utilsServices.GetUserResponse(T("Your {0} account has been attached to your local account.", authResult.Provider).Text, _identityProviders));
                        }
                    }

                    if (_openAuthMembershipServices.CanRegister() && masterUser == null) {
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
                            return _utilsServices.ConvertToJsonResult(result);
                        }
                        else {
                            // Handle LoggedIn Event
                            _userEventHandler.LoggedIn(newUser);
                            return _utilsServices.ConvertToJsonResult(_utilsServices.GetUserResponse(T("You have been logged in using your {0} account. We have created a local account for you with the name '{1}'", authResult.Provider, newUser.UserName).Text, _identityProviders));
                        }
                    }
                    result = _utilsServices.GetResponse(ResponseType.None, "Login failed.");
                    return _utilsServices.ConvertToJsonResult(result);
                }
            }
            catch (Exception e) {
                result = _utilsServices.GetResponse(ResponseType.None, e.Message);
                return _utilsServices.ConvertToJsonResult(result);
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