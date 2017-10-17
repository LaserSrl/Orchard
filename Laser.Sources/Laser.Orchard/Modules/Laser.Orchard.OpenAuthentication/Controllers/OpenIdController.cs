using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
using DotNetOpenAuth.OpenId.RelyingParty;
using Laser.Orchard.OpenAuthentication.Models;
using Laser.Orchard.OpenAuthentication.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc.Extensions;
using Orchard.Security;
using Orchard.Themes;
using Orchard.UI.Notify;
using Orchard.Users.Events;
using Orchard.Users.Models;
using Orchard.Users.Services;

namespace Laser.Orchard.OpenAuthentication.Controllers {
    [Themed]
    [OrchardFeature("Laser.Orchard.OpenAuthentication.AppDirect")]
    public class OpenIdController : Controller {
        private readonly INotifier _notifier;
        private readonly IUserEventHandler _userEventHandler;
        private readonly IOrchardServices _orchardServices;
        private readonly IMembershipService _membershipService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IUserProviderServices _userProviderServices;

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }
        public OpenIdController(
            INotifier notifier,
            IMembershipService membershipService,
            IAuthenticationService authenticationService,
            IOrchardServices orchardServices,
            IUserEventHandler userEventHandler,
            IUserProviderServices userProviderServices
            ) {
            _userProviderServices = userProviderServices;
            _userEventHandler = userEventHandler;
            _notifier = notifier;
            _membershipService = membershipService;
            _authenticationService = authenticationService;
            _orchardServices = orchardServices;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;

        }
        public ActionResult AppDirectLogOn() {
            string stropenid = Request.QueryString["openid"];
            var baseurl = _orchardServices.WorkContext.CurrentSite.As<OpenAuthenticationSettingsPart>().AppDirectBaseUrl ?? "";
            if (stropenid != null && stropenid.ToLower().StartsWith(baseurl.ToLower() + "/openid/id")) {
                string accountIdentifier = Request.QueryString["accountIdentifier"];
                OpenIdRelyingParty rpopenid = new OpenIdRelyingParty();
                var response = rpopenid.GetResponse();
                if (response != null) {
                    switch (response.Status) {
                        case AuthenticationStatus.Authenticated:
                            var extradata = response.GetExtension<ClaimsResponse>();
                            // var email = extradata.Email;
                            var email = extradata.MailAddress.ToString();
                            #region UserSearch  

                            var orchardUserName = FindOrTransformUserName(email,"AppDirect");

                            #endregion
                            //_userProviderServices.Create("AppDirect",)
                            // string identifierclaim = response.ClaimedIdentifier.ToString();
                            //   string orchardUserName = ("AppDirect_" + email).ToLowerInvariant();
                            if (Login(orchardUserName)) {
                                _notifier.Information(T("You have been logged using your OpenId account."));
                                return this.RedirectLocal(null, "~/admin");
                            }
                            else {
                                Logger.Error("User {0} Authenticated by OpenId is denied to access", orchardUserName);
                                return this.RedirectLocal(null, "~/admin");
                            }
                            break;
                        case AuthenticationStatus.Canceled:
                            Logger.Error("User {0}: generate event Canceled", accountIdentifier);
                            _notifier.Error(T("Your authentication request canceled."));
                            return this.RedirectLocal(null, "~/admin");
                            break;
                        case AuthenticationStatus.Failed:
                            Logger.Error("User {0}: generate event Failed", accountIdentifier);
                            _notifier.Error(T("Your authentication request failed."));
                            return this.RedirectLocal(null, "~/admin");
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
        private string TransformLocalUserToprovidernameUser(UserPart localuser,string providername, int progress = 0) {
            var userdata = localuser.Email.Substring(0, localuser.Email.IndexOf('@'));
            if (progress > 0)
                userdata = progress.ToString() + userdata;
            if (! _orchardServices.ContentManager
              .Query<UserPart, UserPartRecord>()
              .Where(u => u.UserName==userdata)
              .List().Any()) {
                localuser.UserName = userdata;
                localuser.NormalizedUserName = userdata.ToLowerInvariant();
                _userProviderServices.Create(providername, localuser.Email, localuser, userdata);
                return userdata;
            }
            else
                return TransformLocalUserToprovidernameUser(localuser, providername, progress + 1);
        }
        private string FindOrTransformUserName(string email,string providername) {
            var username = "";
            var prov = _userProviderServices.Get(providername, email.ToLower());
            if (prov != null)
                username = prov.ProviderUserData; // userName
            else {
                var users = _orchardServices.ContentManager
               .Query<UserPart, UserPartRecord>()
               .Where(u => u.Email==email).List();
                if (users != null) {
                    foreach (var user in users) {
                        if (!_userProviderServices.Get(user.Id).Any()) { //Utente locale
                            username = TransformLocalUserToprovidernameUser(user, providername);
                            break;
                        }
                    }
                }
            }
            return username;
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

        //private bool CreateUserOrchard(string username, string email) {
        //    try {
        //        string password = Membership.GeneratePassword(10, 5);
        //        var newuser = _membershipService.CreateUser(new CreateUserParams(username, password, email, this.T.Invoke("Auto Registered User", new object[0]).Text, password, true));
        //        if (newuser != null) {
        //            _userProviderServices.Create("AppDirect",)
        //            return true;
        //        }
        //        Logger.Error(string.Format("AppDirect => Error Creating user username={0} email={1}", (object)username, (object)email));
        //        return false;
        //    }
        //    catch (Exception ex) {
        //        Logger.Error(string.Format("AppDirect => Error Creating user username={0} email={1}", (object)username, (object)email) + " " + ex.Message);
        //        return false;
        //    }
        //}
    }
}