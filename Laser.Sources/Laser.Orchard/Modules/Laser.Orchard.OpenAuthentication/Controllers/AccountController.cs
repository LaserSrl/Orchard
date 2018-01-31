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

            // At this point, login using the OpenAuth provider failed, meaning that we could not find a match
            // between the information from the provider and Orchard's users.

            // Get additional UserData
            if (result.ExtraData.ContainsKey("accesstoken")) {
                result = _openAuthClientProvider.GetUserData(result.Provider, result, result.ExtraData["accesstoken"]);
            } else {
                result = _openAuthClientProvider.GetUserData(result.Provider, result, "");
            }
            // _openAuthClientProvider.GetUserData(params) may return null if there is no configuration for a provider
            // with the given name.
            if (result == null) {
                // handle this condition and exit the method
                _notifier.Error(T("Your authentication request failed."));

                return new RedirectResult(Url.LogOn(returnUrl));
            }

            // _openAuthClientProvider.NormalizeData(params) may return null if there is no configuration for a provider
            // with the given name. If result != null, that is not the case, because in that condition GetUserData(params)
            // would return null, and we would have already exited the method.
            var userParams = _openAuthClientProvider.NormalizeData(result.Provider, new OpenAuthCreateUserParams(result.UserName,
                                                        result.Provider,
                                                        result.ProviderUserId,
                                                        result.ExtraData));

            var temporaryUser = _openAuthMembershipServices.CreateTemporaryUser(userParams);

            // In what condition can GetAuthenticatedUser() not be null? To reach this code, _orchardOpenAuthWebSecurity.Login(params)
            // must have returned false. That happens if there was no record for the combination Provider/ProviderUserId, or if
            // GetAuthenticatedUser() returned null in it. In the latter case, it should still return null. The former case means
            // we are trying to login with an OAuth provider and it's the first time we are calling it for this user, but we are 
            // also already authenticated in some other way. This only makes sense in a situation where, as authenticated users,
            // we are allowed to add information from OAuth providers to our account: Users/Account/LogOn, if the user is authenticated,
            // redirects to the homepage, and does not give an option to go and login again using OAuth.
            var masterUser = _authenticationService.GetAuthenticatedUser()
                ?? _orchardOpenAuthWebSecurity.GetClosestMergeableKnownUser(temporaryUser);
            // The authenticated User or depending from settings the first created user with the same e-mail

            if (masterUser != null) {
                // If the current user is logged in or settings ask for a user merge and we found a User with the same email 
                // create or merge accounts
                _orchardOpenAuthWebSecurity.CreateOrUpdateAccount(result.Provider, result.ProviderUserId,
                                                                  masterUser, result.ExtraData.ToJson());

                _notifier.Information(T("Your {0} account has been attached to your local account.", result.Provider));

                if (_authenticationService.GetAuthenticatedUser() != null) { // if the user was already logged in 
                    // here masterUser == _authenticationService.GetAuthenticatedUser()
                    return this.RedirectLocal(returnUrl);
                }
            }

            if (_openAuthMembershipServices.CanRegister() && masterUser == null) { 
                // User can register and there is not a user with the same email
                var createUserParams = new OpenAuthCreateUserParams(result.UserName,
                                                                    result.Provider,
                                                                    result.ProviderUserId,
                                                                    result.ExtraData);
                createUserParams = _openAuthClientProvider.NormalizeData(result.Provider, createUserParams);
                // Creating the user here calls the IMembershipService, that will take care of invoking the user events
                var newUser = _openAuthMembershipServices.CreateUser(createUserParams);
                // newUser may be null here, if creation of a new user fails.
                // TODO: we should elsewhere add an UserEventHandler that in the Creating event handles the case where
                // here we are trying to create a user with the same Username or Email as an existing one. That would simply
                // use IUserService.VerifyUnicity(username, email)
                _orchardOpenAuthWebSecurity.CreateOrUpdateAccount(result.Provider,
                                                                  result.ProviderUserId,
                                                                  newUser,
                                                                  result.ExtraData.ToJson());
                // The default implementation of IOpendAuthMembershipService creates a disabled user.
                // This next call to ApproveUser is here, so that in the event handlers we have that the records for the
                // OAuth provider is populated.
                _openAuthMembershipServices.ApproveUser(newUser);
                _authenticationService.SignIn(newUser, false);

                if (newUser != null) {
                    _notifier.Information(
                        T("You have been logged in using your {0} account. We have created a local account for you with the name '{1}'", result.Provider, newUser.UserName));
                    _userEventHandler.LoggedIn(newUser);
                } else
                    _notifier.Error(T("Your authentication request failed."));

                return this.RedirectLocal(returnUrl);
            } else if (masterUser != null) {
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
                // authResult may be null if the provider name matches no configured provider
                if (authResult == null || !authResult.IsSuccessful) {
                    result = _utilsServices.GetResponse(ResponseType.InvalidUser, "Token authentication failed.");
                    return _utilsServices.ConvertToJsonResult(result);
                } else {

                    if (_orchardOpenAuthWebSecurity.Login(authResult.Provider, authResult.ProviderUserId)) {
                        // Login may right now succeed for disabled users
                        if (HttpContext.Response.Cookies.Count == 0) {
                            result = _utilsServices.GetResponse(ResponseType.None, "Unable to send back a cookie.");
                            return _utilsServices.ConvertToJsonResult(result);
                        } else {
                            authenticatedUser = _authenticationService.GetAuthenticatedUser();
                            // If the user is disabled, we would still get it as authenticated here, but GetAuthenticatedUser() would 
                            // return null on the next request.
                            _userEventHandler.LoggedIn(authenticatedUser);
                            return _utilsServices.ConvertToJsonResult(_utilsServices.GetUserResponse("", _identityProviders));
                        }
                    } else {
                        // Login returned false: either the user given by Provider+UserId has never been registered (so we have no
                        // matching username to use), or no user exists in Orchard with that username, or SignIn failed somehow.

                        // _openAuthClientProvider.NormalizeData(params) may return null if there is no configuration for a provider
                        // with the given name. If result != null, that is not the case, because in that condition GetUserData(params)
                        // would return null, and we would have already exited the method.
                        var userParams = _openAuthClientProvider.NormalizeData(authResult.Provider, new OpenAuthCreateUserParams(authResult.UserName,
                                                                    authResult.Provider,
                                                                    authResult.ProviderUserId,
                                                                    authResult.ExtraData));

                        var temporaryUser = _openAuthMembershipServices.CreateTemporaryUser(userParams);

                        // This is an attempt to login using an OAuth provider. The call to .Login(params) returned false. In an actual 
                        // login there is no reason why GetAuthenticatedUser() should return a user, unless we are in a situation where,
                        // as authenticated users, we are allowed to add information from OAuth providers to our account

                        // The authenticated User or depending from settings the first created user with the same e-mail
                        masterUser = _authenticationService.GetAuthenticatedUser() 
                            ?? _orchardOpenAuthWebSecurity.GetClosestMergeableKnownUser(temporaryUser);

                        authenticatedUser = _authenticationService.GetAuthenticatedUser();
                    }
                    // We are here, so we must have gone through the branch for failed login.
                    
                    if (masterUser != null) {
                        // If the current user is logged in or settings ask for a user merge and we found a User with the same email 
                        // create or merge accounts
                        _orchardOpenAuthWebSecurity.CreateOrUpdateAccount(authResult.Provider, authResult.ProviderUserId,
                                                                          masterUser, authResult.ExtraData.ToJson());
                        // Handle LoggedIn Event
                        if (authenticatedUser == null) {
                            _authenticationService.SignIn(masterUser, false);
                        }
                        _userEventHandler.LoggedIn(masterUser);
                        return _utilsServices.ConvertToJsonResult(_utilsServices.GetUserResponse(T("Your {0} account has been attached to your local account.", authResult.Provider).Text, _identityProviders));

                    }

                    if (_openAuthMembershipServices.CanRegister() && masterUser == null) {
                        // User can register and there is not a user with the same email
                        var createUserParams = new OpenAuthCreateUserParams(authResult.UserName,
                                                                            authResult.Provider,
                                                                            authResult.ProviderUserId,
                                                                            authResult.ExtraData);
                        createUserParams = _openAuthClientProvider.NormalizeData(authResult.Provider, createUserParams);
                        // Creating the user here calls the IMembershipService, that will take care of invoking the user events
                        var newUser = _openAuthMembershipServices.CreateUser(createUserParams);
                        // newUser may be null here, if creation of a new user fails.
                        // TODO: we should elsewhere add an UserEventHandler that in the Creating event handles the case where
                        // here we are trying to create a user with the same Username or Email as an existing one. That would simply
                        // use IUserService.VerifyUnicity(username, email)
                        _orchardOpenAuthWebSecurity.CreateOrUpdateAccount(authResult.Provider,
                                                                          authResult.ProviderUserId,
                                                                          newUser, authResult.ExtraData.ToJson());
                        // The default implementation of IOpendAuthMembershipService creates a disabled user.
                        // This next call to ApproveUser is here, so that in the event handlers we have that the records for the
                        // OAuth provider is populated.
                        _openAuthMembershipServices.ApproveUser(newUser);
                        _authenticationService.SignIn(newUser, false);

                        if (HttpContext.Response.Cookies.Count == 0) {
                            // SignIn adds the authentication cookie to the response, so that is what we are checking here
                            result = _utilsServices.GetResponse(ResponseType.None, "Unable to send back a cookie.");
                            return _utilsServices.ConvertToJsonResult(result);
                        } else {
                            // Handle LoggedIn Event
                            _userEventHandler.LoggedIn(newUser);
                            return _utilsServices.ConvertToJsonResult(_utilsServices.GetUserResponse(T("You have been logged in using your {0} account. We have created a local account for you with the name '{1}'", authResult.Provider, newUser.UserName).Text, _identityProviders));
                        }
                    }

                    result = _utilsServices.GetResponse(ResponseType.None, "Login failed.");
                    return _utilsServices.ConvertToJsonResult(result);
                }
            } catch (Exception e) {
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