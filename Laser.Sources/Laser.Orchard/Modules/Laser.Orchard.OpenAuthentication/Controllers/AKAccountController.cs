using Laser.Orchard.OpenAuthentication.Services;
using Laser.Orchard.StartupConfig.IdentityProvider;
using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.StartupConfig.WebApiProtection.Filters;
using Orchard.Security;
using Orchard.Users.Events;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Laser.Orchard.OpenAuthentication.Controllers {
    public class AKAccountController : BaseAccountController {

        public AKAccountController(
            IUtilsServices utilsServices,
            IOrchardOpenAuthClientProvider openAuthClientProvider,
            IOrchardOpenAuthWebSecurity orchardOpenAuthWebSecurity,
            IEnumerable<IIdentityProvider> identityProviders,
            IOpenAuthMembershipServices openAuthMembershipServices,
            IAuthenticationService authenticationService,
            IUserEventHandler userEventHandler) : base(
                utilsServices,
                openAuthClientProvider,
                orchardOpenAuthWebSecurity,
                identityProviders,
                openAuthMembershipServices,
                authenticationService,
                userEventHandler
                ) { }

        [OutputCache(NoStore = true, Duration = 0)]
        [AlwaysAccessible]
        [WebApiKeyFilterForControllers(true)]
        public ContentResult ExternalTokenLogOn(string __provider__, string token, string secret = "") {
            return ExternalTokenLogOnLogic(__provider__, token, secret);
        }

        [OutputCache(NoStore = true, Duration = 0)]
        [AlwaysAccessible]
        [WebApiKeyFilterForControllers(true)]
        public ContentResult ExternalTokenLogOnSsl(string __provider__, string token, string secret = "") {
            return ExternalTokenLogOnLogic(__provider__, token, secret);
        }

    }
}