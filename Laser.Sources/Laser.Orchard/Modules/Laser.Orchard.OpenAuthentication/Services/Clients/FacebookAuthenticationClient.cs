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

namespace Laser.Orchard.OpenAuthentication.Services.Clients {
    public class FacebookAuthenticationClient : IExternalAuthenticationClient {
        public ILogger _logger { get; set; }

        public FacebookAuthenticationClient() {
            _logger =NullLogger.Instance;
        }
        public string ProviderName {
            get { return "facebook"; }
        }

        public IAuthenticationClient Build(ProviderConfigurationRecord providerConfigurationRecord) {
           // return new FacebookClient(providerConfigurationRecord.ProviderIdKey, providerConfigurationRecord.ProviderSecret);
            string ClientId = providerConfigurationRecord.ProviderIdKey;
            string ClientSecret = providerConfigurationRecord.ProviderSecret;

            var client = new FacebookOAuth2Client(ClientId, ClientSecret);

            //var client2 = new GoogleOpenIdClient();
            //return client2;
            return client;

        }

        public AuthenticationResult GetUserData(ProviderConfigurationRecord clientConfiguration, string userAccessToken, string userAccessSecret="") {
            var serializer = new DataContractJsonSerializer(typeof(FacebookGraphData));
            FacebookGraphData graphData;
            var request =
                WebRequest.Create(
                    "https://graph.facebook.com/me?fields=id,email,birthday,first_name,last_name,name,locale,link,gender,timezone,updated_time,verified&access_token=" + userAccessToken);
            try {
                using (var response = request.GetResponse()) {
                    using (var responseStream = response.GetResponseStream()) {

                        graphData = (FacebookGraphData)serializer.ReadObject(responseStream);
                    }
                }
            } catch {
                return AuthenticationResult.Failed;
            }
            // this dictionary must contains 
            var userData = new Dictionary<string, string>();
            userData["id"] = graphData.Id;
            userData["username"] = graphData.Name;
            userData["mail"] = graphData.Email;
            userData["name"] = graphData.Name;
            userData["link"] = graphData.Link == null ? null : graphData.Link.AbsoluteUri;
            userData["gender"] = graphData.Gender;
            userData["birthday"] = graphData.Birthday;

            if (userData == null) {
                return AuthenticationResult.Failed;
            }

            string id = userData["id"];
            string name;

            // Some oAuth providers do not return value for the 'username' attribute. 
            // In that case, try the 'name' attribute. If it's still unavailable, fall back to 'id'
            if (!userData.TryGetValue("username", out name) && !userData.TryGetValue("name", out name)) {
                name = id;
            }

            // add the access token to the user data dictionary just in case page developers want to use it
            userData["accesstoken"] = userAccessToken;

            return new AuthenticationResult(
                isSuccessful: true, provider: this.ProviderName, providerUserId: id, userName: name, extraData: userData);
        }


        /// <summary>
        /// 
        /// 
        /// </summary>
        /// <param name="createUserParams"></param>
        /// <returns></returns>
        public OpenAuthCreateUserParams NormalizeData(OpenAuthCreateUserParams createUserParams) 
        {
            OpenAuthCreateUserParams retVal;

            retVal = createUserParams;
            string emailAddress = string.Empty;
           
            var valoriRicavati = createUserParams.ExtraData.Keys;

            foreach (KeyValuePair<string, string> values in createUserParams.ExtraData) {
                if (values.Key == "mail" || values.Key == "email") 
                {
                    retVal.UserName = values.Value.IsEmailAddress() ? values.Value.Substring(0, values.Value.IndexOf('@')) : values.Value; 
                }                  
            }
                   
            // if (!Regex.IsMatch(retVal.UserName, "^[A-Za-z0-9]")) 
                   

            return retVal;
       
       }


     }
  }
