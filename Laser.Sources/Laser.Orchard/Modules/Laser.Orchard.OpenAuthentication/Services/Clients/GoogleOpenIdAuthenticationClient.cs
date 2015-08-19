using DotNetOpenAuth.AspNet;
using DotNetOpenAuth.AspNet.Clients;
using Laser.Orchard.OpenAuthentication.Models;

namespace Laser.Orchard.OpenAuthentication.Services.Clients {
    public class GoogleOpenIdAuthenticationClient : IExternalAuthenticationClient {
        public string ProviderName {
            get { return "Google"; }
        }

        public IAuthenticationClient Build(ProviderConfigurationRecord providerConfigurationRecord) {
            return new GoogleOpenIdClient();
        }


        public AuthenticationResult GetUserData(ProviderConfigurationRecord clientConfiguration, string userAccessToken, string userAccessSecret = "") {
            throw new System.NotImplementedException();
        }
    }
}