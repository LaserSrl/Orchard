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
            string ClientId = providerConfigurationRecord.ProviderIdKey;
            string ClientSecret = providerConfigurationRecord.ProviderSecret;

            var client = new LinkedInOAuth2Client(ClientId, ClientSecret);

            return client;
        }


        public AuthenticationResult GetUserData(ProviderConfigurationRecord clientConfiguration, string userAccessToken, string userAccessSecret = "") {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="createUserParams"></param>
        /// <returns></returns>
        public OpenAuthCreateUserParams NormalizeData(OpenAuthCreateUserParams createUserParams) {
            OpenAuthCreateUserParams retVal;

            retVal = createUserParams;
            string emailAddress = string.Empty;

            var valoriRicavati = createUserParams.ExtraData.Values;
            int countVal = 0;

            foreach (string valric in valoriRicavati) {
                if (countVal == 3) {
                    emailAddress = valric;
                    retVal.UserName = emailAddress;
                }

                countVal = countVal + 1;
            }

            return retVal;

        }

    }
}