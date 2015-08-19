using DotNetOpenAuth.AspNet;
using DotNetOpenAuth.AspNet.Clients;
using Laser.Orchard.OpenAuthentication.Models;

namespace Laser.Orchard.OpenAuthentication.Services.Clients {
    public class MicrosoftAuthenticationClient : IExternalAuthenticationClient {
        public string ProviderName {
            get { return "Microsoft"; }
        }

        public IAuthenticationClient Build(ProviderConfigurationRecord providerConfigurationRecord) {
            return new MicrosoftClient(providerConfigurationRecord.ProviderIdKey, providerConfigurationRecord.ProviderSecret);
        }


        public AuthenticationResult GetUserData(ProviderConfigurationRecord clientConfiguration, string userAccessToken, string userAccessSecret = "") {
            throw new System.NotImplementedException();
        }
    }
}