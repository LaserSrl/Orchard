using DotNetOpenAuth.AspNet;
using DotNetOpenAuth.AspNet.Clients;
using Laser.Orchard.OpenAuthentication.Services.Clients;
using Orchard;
using Orchard.Mvc;
using Orchard.Security;
using Orchard.Users.Events;
using System.Collections.Generic;
using System.Web;

namespace Laser.Orchard.OpenAuthentication.Services {
    public interface IOpenAuthSecurityManagerWrapper : IDependency {
        bool Login(string providerUserId, bool createPersistentCookie);
        AuthenticationResult VerifyAuthentication(string returnUrl);
        void RequestAuthentication(string providerName, string returnUrl);
    }

    public class OpenAuthSecurityManagerWrapper : IOpenAuthSecurityManagerWrapper {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOrchardOpenAuthClientProvider _orchardOpenAuthClientProvider;
        private readonly IOrchardOpenAuthDataProvider _orchardOpenAuthDataProvider;
        private readonly IAuthenticationService _authenticationService;
        private readonly IMembershipService _membershipService;
        private readonly IUserEventHandler _userEventHandler;
        private readonly IEnumerable<IExternalAuthenticationClient> _openAuthAuthenticationClients;

        public OpenAuthSecurityManagerWrapper(IHttpContextAccessor httpContextAccessor, 
                                              IOrchardOpenAuthClientProvider orchardOpenAuthClientProvider,
                                              IOrchardOpenAuthDataProvider orchardOpenAuthDataProvider,
                                              IAuthenticationService authenticationService,
                                              IMembershipService membershipService,
                                              IUserEventHandler userEventHandler,
                                              IEnumerable<IExternalAuthenticationClient> openAuthAuthenticationClients)
        {
            _httpContextAccessor = httpContextAccessor;
            _orchardOpenAuthClientProvider = orchardOpenAuthClientProvider;
            _orchardOpenAuthDataProvider = orchardOpenAuthDataProvider;
            _authenticationService = authenticationService;
            _membershipService = membershipService;
            _userEventHandler = userEventHandler;
            _openAuthAuthenticationClients = openAuthAuthenticationClients;
        }

        private string ProviderName {
            get { return OpenAuthSecurityManager.GetProviderName(_httpContextAccessor.Current()); }
        }



        public bool Login(string providerUserId, bool createPersistentCookie) {
            string userName = _orchardOpenAuthDataProvider.GetUserNameFromOpenAuth(ProviderName, providerUserId);

            if (string.IsNullOrWhiteSpace(userName))
                return false;

            if (_membershipService.GetUser(userName) != null)
                _authenticationService.SignIn(_membershipService.GetUser(userName), createPersistentCookie);

            var authenticatedUser = _authenticationService.GetAuthenticatedUser();

            if (authenticatedUser == null)
                return false;
            else
            {
                _userEventHandler.LoggedIn(authenticatedUser);
                return true;
            }
        }

        public AuthenticationResult VerifyAuthentication(string returnUrl) {
            if (string.IsNullOrEmpty(ProviderName)) {
                // se non è noto il provider richiama il RewriteRequest di tutti i client registrati
                RewriteRequest();
            }
            return SecurityManager(ProviderName).VerifyAuthentication(returnUrl);
        }


        public void RequestAuthentication(string providerName, string returnUrl) {
            SecurityManager(providerName).RequestAuthentication(returnUrl);
        }

        private OpenAuthSecurityManager SecurityManager(string providerName) {
            return new OpenAuthSecurityManager(_httpContextAccessor.Current(), _orchardOpenAuthClientProvider.GetClient(providerName), _orchardOpenAuthDataProvider); 
        }
        private void RewriteRequest() {
            foreach (var client in _openAuthAuthenticationClients) {
                if (client.RewriteRequest()) {
                    break;
                }
            }
        }
    }
}