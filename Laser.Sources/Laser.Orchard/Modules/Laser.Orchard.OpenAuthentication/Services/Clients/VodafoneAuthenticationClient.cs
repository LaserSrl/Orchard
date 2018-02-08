using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using DotNetOpenAuth.AspNet;
using DotNetOpenAuth.AspNet.Clients;
using DotNetOpenAuth.Messaging;
using Laser.Orchard.OpenAuthentication.Models;
using Orchard.Logging;
using Laser.Orchard.OpenAuthentication.Security;
using System.Text.RegularExpressions;
using Laser.Orchard.OpenAuthentication.Extensions;
using System.Collections.Specialized;
using Newtonsoft.Json;
using System.Web;

namespace Laser.Orchard.OpenAuthentication.Services.Clients {
    public class VodafoneAuthenticationClient : IExternalAuthenticationClient {
        public VodafoneAuthenticationClient() {
            Logger = NullLogger.Instance;
        }

        public string ProviderName {
            get { return "Vodafone"; }
        }

        public ILogger Logger { get; set; }

        public IAuthenticationClient Build(ProviderConfigurationRecord providerConfigurationRecord) {
            string ClientId = providerConfigurationRecord.ProviderIdKey;
            string ClientSecret = providerConfigurationRecord.ProviderSecret;
            var client = new OpenAuthVodafoneOAuth2Client(ClientId,ClientSecret);
            return client;
        }

        public OpenAuthCreateUserParams NormalizeData(OpenAuthCreateUserParams createUserParams) {
            OpenAuthCreateUserParams retVal = createUserParams;
            string emailAddress = string.Empty;
            foreach (KeyValuePair<string, string> values in createUserParams.ExtraData) {
                if (values.Key == "email") {
                    retVal.UserName = values.Value.IsEmailAddress() ? values.Value.Substring(0, values.Value.IndexOf('@')) : values.Value;
                }
            }
            return retVal;
        }

        public AuthenticationResult GetUserData(ProviderConfigurationRecord clientConfiguration, AuthenticationResult previosAuthResult, string userAccessToken) {
            var userData = (Build(clientConfiguration) as OpenAuthVodafoneOAuth2Client).GetUserDataDictionary(userAccessToken);
            //Logger.Error("user data count: {0}", userData.Count);
            if (!userData.ContainsKey("email")) { // email non presente
                Logger.Error(string.Format("OpenAuth Vodafone User: token {0} is valid but login was not accepted: no email returned by Vodafone service", userAccessToken));
                return AuthenticationResult.Failed;
            }
            if (!userData.ContainsKey("userid")) { // userid non presente
                Logger.Error(string.Format("OpenAuth Vodafone User: token {0} is valid but login was not accepted: no userid returned by Vodafone service", userAccessToken));
                return AuthenticationResult.Failed;
            }
            if (userData.ContainsKey("mobile"))
                userData["phone"] = userData["mobile"];
            string email = userData["email"];
            string name = "";
            try {
                name = userData["surname"] + " " + userData["name"];
              //  userData["name"] = name;
            }
            catch (Exception) { }
            string id = userData["userid"];
          
            userData["email"] = email;
            return new AuthenticationResult(true, this.ProviderName, id, name, userData);
        }

        public AuthenticationResult GetUserData(ProviderConfigurationRecord clientConfiguration, AuthenticationResult previousAuthResult, string token, string userAccessSecret, string returnUrl) {
            var client = Build(clientConfiguration) as OpenAuthVodafoneOAuth2Client;
            //Logger.Error("Inizio chiamata Google");
            //     string userAccessToken = client.GetAccessToken(new Uri(returnUrl), token);
            string userAccessToken = token;
            //Logger.Error("access token: {0}", userAccessToken);
            return GetUserData(clientConfiguration, previousAuthResult, userAccessToken);
        }

        public bool RewriteRequest() {
            return new ServiceUtility().RewriteRequestByState();
        }
    }
}