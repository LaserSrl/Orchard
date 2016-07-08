using DotNetOpenAuth.AspNet;
using DotNetOpenAuth.AspNet.Clients;
using Laser.Orchard.OpenAuthentication.Models;
using Laser.Orchard.OpenAuthentication.Security;

namespace Laser.Orchard.OpenAuthentication.Services.Clients {
    public class LinkedInAuthenticationClient : IExternalAuthenticationClient {
        public string ProviderName {
            get { return "LinkedIn"; }
        }

        public IAuthenticationClient Build(ProviderConfigurationRecord providerConfigurationRecord) {
            return new LinkedInClient(providerConfigurationRecord.ProviderIdKey, providerConfigurationRecord.ProviderSecret);
        }


        public AuthenticationResult GetUserData(ProviderConfigurationRecord clientConfiguration, string userAccessToken, string userAccessSecret = "") {
            throw new System.NotImplementedException();
        }

        public OpenAuthCreateUserParams NormalizeData(OpenAuthCreateUserParams createUserParams) {
            throw new System.NotImplementedException();
        }
    }
}