using DotNetOpenAuth.AspNet;
using DotNetOpenAuth.AspNet.Clients;
using Laser.Orchard.OpenAuthentication.Models;
using Laser.Orchard.OpenAuthentication.Security;

namespace Laser.Orchard.OpenAuthentication.Services.Clients {
    public class YahooAuthenticationClient : IExternalAuthenticationClient {
        public string ProviderName {
            get { return "Yahoo"; }
        }

        public IAuthenticationClient Build(ProviderConfigurationRecord providerConfigurationRecord) {
            return new YahooOpenIdClient();
        }

        public AuthenticationResult GetUserData(ProviderConfigurationRecord clientConfiguration, AuthenticationResult previosAuthResult, string userAccessToken, string userAccessSecret = "") {
            throw new System.NotImplementedException();
        }

        public OpenAuthCreateUserParams NormalizeData(OpenAuthCreateUserParams createUserParams) {
            throw new System.NotImplementedException();
        }

        public bool RewriteRequest() {
            throw new System.NotImplementedException();
        }
    }
}