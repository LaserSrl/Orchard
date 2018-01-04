using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Laser.Orchard.StartupConfig.WebApiProtection.Filters;
using Newtonsoft.Json.Linq;
using Laser.Orchard.StartupConfig.ViewModels;
using Laser.Orchard.StartupConfig.Services;
using Orchard.Environment.Extensions;
using Laser.Orchard.UsersExtensions.Services;
using Laser.Orchard.MultiStepAuthentication.Services;
using Laser.Orchard.MultiStepAuthentication.DataContracts;
using Orchard.Users.Models;
using Orchard.Security;
using Orchard.Users.Events;
using Laser.Orchard.StartupConfig.IdentityProvider;

namespace Laser.Orchard.MultiStepAuthentication.Controllers {
    [OrchardFeature("Laser.Orchard.NonceLogin")]
    [WebApiKeyFilter(true)]
    public class NonceLoginApiController : BaseMultiStepAccountApiController {

        private readonly IUtilsServices _utilsServices;
        private readonly IUsersExtensionsServices _usersExtensionsServices;
        private readonly INonceService _nonceService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IUserEventHandler _userEventHandler;
        private readonly IEnumerable<IIdentityProvider> _identityProviders;

        public NonceLoginApiController(
            IUtilsServices utilsServices,
            IUsersExtensionsServices usersExtensionsServices,
            INonceService nonceService,
            IAuthenticationService authenticationService,
            IUserEventHandler userEventHandler,
            IEnumerable<IIdentityProvider> identityProviders) {

            _utilsServices = utilsServices;
            _usersExtensionsServices = usersExtensionsServices;
            _nonceService = nonceService;
            _authenticationService = authenticationService;
            _userEventHandler = userEventHandler;
            _identityProviders = identityProviders;
        }
        

        public Response Get(string mail) {
            var user = _usersExtensionsServices.GetUserByMail(mail);

            if (user != null) {
                _nonceService.SendNewOTP(user, DeliveryChannelType.Email);
            }

            return _utilsServices.GetResponse(ResponseType.UnAuthorized);
        }

        public Response Post(JObject message) {
            var msgObj = message.ToObject<NonceLoginMessage>();

            if (msgObj != null) {
                var iuser = _nonceService.UserFromNonce(msgObj.Nonce);
                if (iuser != null) {
                    var user = iuser as UserPart;
                    if (user.RegistrationStatus == UserStatus.Pending) {
                        user.RegistrationStatus = UserStatus.Approved;
                        _authenticationService.SignIn(user, true);
                        _userEventHandler.LoggedIn(user);
                        return _utilsServices.GetUserResponse("", _identityProviders, null);
                    }
                }
            }

            return _utilsServices.GetResponse(ResponseType.UnAuthorized);
        }
    }
}