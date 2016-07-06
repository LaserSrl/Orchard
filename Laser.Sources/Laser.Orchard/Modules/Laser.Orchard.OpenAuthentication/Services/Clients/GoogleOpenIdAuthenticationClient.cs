using DotNetOpenAuth.AspNet;
using DotNetOpenAuth.AspNet.Clients;
using Laser.Orchard.OpenAuthentication.Models;
using System.Collections.Generic;


namespace Laser.Orchard.OpenAuthentication.Services.Clients {
    public class GoogleOpenIdAuthenticationClient : IExternalAuthenticationClient {
        public string ProviderName {
            get { return "Google"; }
        }

        public IAuthenticationClient Build(ProviderConfigurationRecord providerConfigurationRecord) {

            string ClientId = providerConfigurationRecord.ProviderIdKey;
            string ClientSecret = providerConfigurationRecord.ProviderSecret;
            
            var client = new GoogleOAuth2Client(ClientId, ClientSecret);
 
            //var client2 = new GoogleOpenIdClient();
            //return client2;
            return client;
        }


        public AuthenticationResult GetUserData(ProviderConfigurationRecord clientConfiguration, string userAccessToken, string userAccessSecret = "") {
            throw new System.NotImplementedException();
        }
    }
}