using DotNetOpenAuth.AspNet;
using Laser.Orchard.OpenAuthentication.Security;
using Laser.Orchard.OpenAuthentication.Services;
using Laser.Orchard.StartupConfig.IdentityProvider;
using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.StartupConfig.ViewModels;
using Orchard;
using Orchard.Localization;
using Orchard.Mvc.Extensions;
using Orchard.Security;
using Orchard.Users.Events;
using System;
using System.Collections.Generic;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.OpenAuthentication.Controllers {
    public abstract class BaseAccountController : Controller {

        private readonly IUtilsServices _utilsServices;
        private readonly IOrchardOpenAuthClientProvider _openAuthClientProvider;
        private readonly IOrchardOpenAuthWebSecurity _orchardOpenAuthWebSecurity;
        private readonly IEnumerable<IIdentityProvider> _identityProviders;
        private readonly IOpenAuthMembershipServices _openAuthMembershipServices;
        private readonly IAuthenticationService _authenticationService;
        private readonly IUserEventHandler _userEventHandler;

        public BaseAccountController(
            IUtilsServices utilsServices,
            IOrchardOpenAuthClientProvider openAuthClientProvider,
            IOrchardOpenAuthWebSecurity orchardOpenAuthWebSecurity,
            IEnumerable<IIdentityProvider> identityProviders,
            IOpenAuthMembershipServices openAuthMembershipServices,
            IAuthenticationService authenticationService,
            IUserEventHandler userEventHandler) {

            _utilsServices = utilsServices;
            _openAuthClientProvider = openAuthClientProvider;
            _orchardOpenAuthWebSecurity = orchardOpenAuthWebSecurity;
            _identityProviders = identityProviders;
            _openAuthMembershipServices = openAuthMembershipServices;
            _authenticationService = authenticationService;
            _userEventHandler = userEventHandler;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        [OutputCache(NoStore = true, Duration = 0)]
        protected ContentResult ExternalTokenLogOnLogic(string __provider__, string token, string secret = "") {
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
                // authResult may be null if the provider name matches no configured provider
                if (authResult == null || !authResult.IsSuccessful) {
                    result = _utilsServices.GetResponse(ResponseType.InvalidUser, "Token authentication failed.");
                    return _utilsServices.ConvertToJsonResult(result);
                } else {

                    if (_orchardOpenAuthWebSecurity.Login(authResult.Provider, authResult.ProviderUserId)) {
                        // Login also returns false for disabled users (this used to not be the case)
                        if (HttpContext.Response.Cookies.Count == 0) {
                            // For some reason, SignIn failed to add the authentication cookie to the response
                            result = _utilsServices.GetResponse(ResponseType.None, "Unable to send back a cookie.");
                            return _utilsServices.ConvertToJsonResult(result);
                        } else {
                            // The LoggedIn event is already raised in the Login method just before returning true, 
                            // so we should not be raising it here as well.
                            return _utilsServices.ConvertToJsonResult(_utilsServices.GetUserResponse("", _identityProviders));
                        }
                    }
                    // Login returned false: either the user given by Provider+UserId has never been registered (so we have no
                    // matching username to use), or no user exists in Orchard with that username, or SignIn failed somehow, or
                    // the user is disabled.

                    // _openAuthClientProvider.NormalizeData(params) may return null if there is no configuration for a provider
                    // with the given name. If authResult != null, that is not the case, because in that condition GetUserData(params)
                    // would return null, and we would have already exited the method.
                    var userParams = _openAuthClientProvider.NormalizeData(authResult.Provider,
                        new OpenAuthCreateUserParams(authResult.UserName,
                            authResult.Provider,
                            authResult.ProviderUserId,
                            authResult.ExtraData));

                    var temporaryUser = _openAuthMembershipServices.CreateTemporaryUser(userParams);

                    // This is an attempt to login using an OAuth provider. The call to .Login(params) returned false. In an actual 
                    // login there is no reason why GetAuthenticatedUser() should return a user, unless we are in a situation where,
                    // as authenticated users, we are allowed to add information from OAuth providers to our account

                    // The authenticated User or depending from settings the first created user with the same e-mail
                    var masterUser = _authenticationService.GetAuthenticatedUser()
                        ?? _orchardOpenAuthWebSecurity.GetClosestMergeableKnownUser(temporaryUser);

                    var authenticatedUser = _authenticationService.GetAuthenticatedUser();

                    if (masterUser != null) {
                        // If the current user is logged in or settings ask for a user merge and we found a User with the same email 
                        // create or merge accounts
                        _orchardOpenAuthWebSecurity.CreateOrUpdateAccount(authResult.Provider, authResult.ProviderUserId,
                                                                          masterUser, authResult.ExtraData);
                        // Handle LoggedIn Event
                        if (authenticatedUser == null) {
                            _authenticationService.SignIn(masterUser, false);
                            _userEventHandler.LoggedIn(masterUser);
                            // The LoggedIn event is invoked here, because if authenticateUser != null, then it means the user
                            // had already logged in some other time
                        }

                        return _utilsServices
                            .ConvertToJsonResult(_utilsServices
                                .GetUserResponse(T("Your {0} account has been attached to your local account.", authResult.Provider).Text,
                                _identityProviders));

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
                        if (newUser != null) {
                            // CreateOrUpdateAccount causes specific OpenAuth events to fire
                            _orchardOpenAuthWebSecurity.CreateOrUpdateAccount(authResult.Provider,
                               authResult.ProviderUserId,
                               newUser,
                               authResult.ExtraData);
                            // The default implementation of IOpendAuthMembershipService creates an approved user.
                            // The events specific to open auth give points to attach handlers where the UserProviderRecord 
                            // is populated correctly.

                            _authenticationService.SignIn(newUser, false);

                            if (HttpContext.Response.Cookies.Count == 0) {
                                // SignIn adds the authentication cookie to the response, so that is what we are checking here
                                // We should never be here executing this code.
                                result = _utilsServices.GetResponse(ResponseType.None, "Unable to send back a cookie.");
                                return _utilsServices.ConvertToJsonResult(result);
                            } else {
                                // Handle LoggedIn Event
                                _userEventHandler.LoggedIn(newUser);
                                return _utilsServices.ConvertToJsonResult(_utilsServices.GetUserResponse(T("You have been logged in using your {0} account. We have created a local account for you with the name '{1}'", authResult.Provider, newUser.UserName).Text, _identityProviders));
                            }
                        }
                        // if newUser == null, just go ahead and return the "Login Failed" Response
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