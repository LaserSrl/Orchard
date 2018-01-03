using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.StartupConfig.ViewModels;
using Laser.Orchard.StartupConfig.WebApiProtection.Filters;
using Laser.Orchard.UsersExtensions.Services;
using Orchard.ContentManagement;
using Orchard.Users.Services;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Orchard.Messaging.Services;
using Orchard.Email.Services;
using Orchard;
using Orchard.Email.Models;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Security;
using Orchard.Users.Events;
using Newtonsoft.Json.Linq;
using Laser.Orchard.UsersExtensions.DataContracts;
using Laser.Orchard.StartupConfig.WebApiProtection.Models;
using Laser.Orchard.StartupConfig.IdentityProvider;

namespace Laser.Orchard.UsersExtensions.Controllers {
    [WebApiKeyFilter(true)]
    public class NonceLoginController : ApiController {
        private readonly IUtilsServices _utilsServices;
        private readonly IUserService _userService;
        private readonly IUsersExtensionsServices _usersExtensionsServices;
        private readonly IMessageService _messageService;
        private readonly IOrchardServices _orchardServices;
        private readonly IAuthenticationService _authenticationService;
        private readonly IUserEventHandler _userEventHandler;
        private readonly IEnumerable<IIdentityProvider> _identityProviders;
        public Localizer T { get; set; }
        public ILogger Log { get; set; }
        public NonceLoginController(IUtilsServices utilsServices, IUserService userService, IUsersExtensionsServices usersExtensionsServices, IMessageService messageService, IOrchardServices orchardServices, IAuthenticationService authenticationService, IUserEventHandler userEventHandler, IEnumerable<IIdentityProvider> identityProviders) {
            _utilsServices = utilsServices;
            _userService = userService;
            _usersExtensionsServices = usersExtensionsServices;
            _messageService = messageService;
            _orchardServices = orchardServices;
            _authenticationService = authenticationService;
            _userEventHandler = userEventHandler;
            _identityProviders = identityProviders;
            T = NullLocalizer.Instance;
            Log = NullLogger.Instance;
        }
        public Response Get(string mail) {
            var user = _usersExtensionsServices.GetUserByMail(mail);
            if(user != null) {
                if(user.RegistrationStatus == UserStatus.Pending) {
                    var currentSite = _orchardServices.WorkContext.CurrentSite;
                    var settings = currentSite.As<Models.NonceLoginSettingsPart>();
                    var nonce = _userService.CreateNonce(user, new TimeSpan(0, settings.NonceMinutesValidity, 0));
                    // send nonce by mail
                    var data = new Dictionary<string, object>();
                    var smtp = currentSite.As<SmtpSettingsPart>();
                    data.Add("Subject", T("{0} - Login", currentSite.SiteName).Text);
                    //var protectionSettings = currentSite.As<ProtectionSettingsPart>();
                    var link = string.Format(settings.LoginLinkFormat ?? "", nonce);
                    data.Add("Body", T("To login on \"{0}\", please open the following link: {1}", currentSite.SiteName, link).Text);
                    data.Add("Recipients", user.Email);
                    //data.Add("Bcc", smtp.Address);
                    _messageService.Send(SmtpMessageChannel.MessageType, data);
                    return _utilsServices.GetResponse(ResponseType.Success);
                }
            }
            return _utilsServices.GetResponse(ResponseType.UnAuthorized);
        }
        public Response Post(JObject message) {
            var msgObj = message.ToObject<NonceLoginMessage>();
            if(msgObj != null) {
                var iuser = _userService.ValidateChallenge(msgObj.Nonce);
                if (iuser != null) {
                    var user = iuser as UserPart;
                    if (user.RegistrationStatus == UserStatus.Pending) {
                        if(string.Equals(user.Email, msgObj.Mail, StringComparison.InvariantCultureIgnoreCase)) {
                            user.RegistrationStatus = UserStatus.Approved;
                            _authenticationService.SignIn(user, true);
                            _userEventHandler.LoggedIn(user);
                            return _utilsServices.GetUserResponse("", _identityProviders, null);
                        }
                    }
                }
            }
            return _utilsServices.GetResponse(ResponseType.UnAuthorized);
        }
    }
}