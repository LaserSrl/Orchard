using DotNetOpenAuth.AspNet;
using DotNetOpenAuth.AspNet.Clients;
using Laser.Orchard.OpenAuthentication.Models;
using Laser.Orchard.OpenAuthentication.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Laser.Orchard.OpenAuthentication.Services.Clients {
    public class MicrosoftAuthenticationClient : IExternalAuthenticationClient {
        public string ProviderName {
            get { return "Microsoft"; }
        }

        public IAuthenticationClient Build(ProviderConfigurationRecord providerConfigurationRecord) {
            return new MicrosoftClient(providerConfigurationRecord.ProviderIdKey, providerConfigurationRecord.ProviderSecret);
        }

        public AuthenticationResult GetUserData(ProviderConfigurationRecord clientConfiguration, AuthenticationResult previousAuthResult, string userAccessToken) {
            Dictionary<string, string> userData = new Dictionary<string, string>();
            string uri = "https://apis.live.net/v5.0/me?access_token=" + userAccessToken;
            var webRequest = (HttpWebRequest)WebRequest.Create(uri);

            using (var webResponse = webRequest.GetResponse()) {
                using (var stream = webResponse.GetResponseStream()) {
                    if (stream == null)
                        return null;

                    using (var textReader = new StreamReader(stream)) {
                        var json = textReader.ReadToEnd();
                        userData = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    }
                }
            }

            string id = userData["id"];
            string name = userData["name"];

            // add the access token to the user data dictionary just in case page developers want to use it
            userData["accesstoken"] = userAccessToken;
            return new AuthenticationResult(true, ProviderName, id, name, userData);
        }

        public AuthenticationResult GetUserData(ProviderConfigurationRecord clientConfiguration, AuthenticationResult previosAuthResult, string token, string userAccessSecret, string returnUrl) {
            return GetUserData(clientConfiguration, previosAuthResult, token);
        }

        public OpenAuthCreateUserParams NormalizeData(OpenAuthCreateUserParams createUserParams) {
            return createUserParams;
        }

        public bool RewriteRequest() {
            return false;
        }
    }
}