using System;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
using DotNetOpenAuth.OpenId.RelyingParty;
using Orchard;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Security;
using Orchard.Themes;
using Orchard.UI.Notify;
using Orchard.Users.Events;

namespace Laser.Orchard.OpenAuthentication.Controllers {
    [Themed]
    [OrchardFeature("Laser.Orchard.OpenAuthentication.AppDirect")]
    public class OpenIdController : Controller {
        private readonly INotifier _notifier;
        private readonly IUserEventHandler _userEventHandler;
        private readonly IOrchardServices _orchardServices;
        private readonly IMembershipService _membershipService;
        private readonly IAuthenticationService _authenticationService;

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }
        public OpenIdController(
            INotifier notifier,
            IMembershipService membershipService,
            IAuthenticationService authenticationService,
            IOrchardServices orchardServices,
            IUserEventHandler userEventHandler) {
            _userEventHandler = userEventHandler;
            _notifier = notifier;
            _membershipService = membershipService;
            _authenticationService = authenticationService;
            _orchardServices = orchardServices;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }
        public ActionResult LogOn() {
            string stropenid = Request.QueryString["openid"];
            if (stropenid != null && stropenid.StartsWith("https://marketplace.appdirect.com/openid/id")) {
                string accountIdentifier = Request.QueryString["accountIdentifier"];
                OpenIdRelyingParty rpopenid = new OpenIdRelyingParty();
                var response = rpopenid.GetResponse();
                if (response != null) {
                    switch (response.Status) {
                        case AuthenticationStatus.Authenticated:
                            var extradata = response.GetExtension<ClaimsResponse>();
                            // var email = extradata.Email;
                            var email = extradata.MailAddress.ToString();
                           // string identifierclaim = response.ClaimedIdentifier.ToString();
                            string orchardUserName = ("AppDirect_" + email).ToLowerInvariant();
                            if (Login(orchardUserName)) {
                                _notifier.Information(T("You have been logged using your OpenId account."));
                                return null;
                            }
                            else {
                                // TODO : Add test if config.autogenerateuser then CreateUserOrchard
                                Logger.Error("User {0} Authenticated by OpenId is denied to access", orchardUserName);
                                return this.RedirectToAction("LogOn");
                            }
                            break;
                        case AuthenticationStatus.Canceled:
                            Logger.Error("User {0}: generate event Canceled", accountIdentifier);

                            _notifier.Error(T("Your authentication request canceled."));
                            return this.RedirectToAction("LogOn");
                            break;
                        case AuthenticationStatus.Failed:
                            Logger.Error("User {0}: generate event Failed", accountIdentifier);
                            _notifier.Error(T("Your authentication request failed."));
                            return this.RedirectToAction("LogOn");
                            break;
                    }

                }

                using (OpenIdRelyingParty openIdRelyingParty = new OpenIdRelyingParty()) {
                    IAuthenticationRequest request = openIdRelyingParty.CreateRequest(stropenid);
                    request.AddExtension(new ClaimsRequest {
                        Email = DemandLevel.Request,
                        Nickname = DemandLevel.Request
                    });
                    request.RedirectToProvider();
                }
            }
            return null;
        }

        private bool Login(string userName) {
            if (string.IsNullOrWhiteSpace(userName))
                return false;
            var user = _membershipService.GetUser(userName);
            if (user != null)
                _authenticationService.SignIn(user, true);
            var authenticatedUser = _authenticationService.GetAuthenticatedUser();
            if (authenticatedUser == null)
                return false;
            _userEventHandler.LoggedIn(authenticatedUser);
            return true;
        }

        private bool CreateUserOrchard(string username, string email) {
            try {
                string password = Membership.GeneratePassword(10, 5);
                if (_membershipService.CreateUser(new CreateUserParams(username, password, email, this.T.Invoke("Auto Registered User", new object[0]).Text, password, true)) != null)
                    return true;
                Logger.Error(string.Format("AppDirect => Error Creating user username={0} email={1}", (object)username, (object)email));
                return false;
            }
            catch (Exception ex) {
                Logger.Error(string.Format("AppDirect => Error Creating user username={0} email={1}", (object)username, (object)email) + " " + ex.Message);
                return false;
            }
        }
    }
}