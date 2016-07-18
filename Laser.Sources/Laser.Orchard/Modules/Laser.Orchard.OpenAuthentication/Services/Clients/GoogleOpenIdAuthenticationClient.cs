using DotNetOpenAuth.AspNet;
using DotNetOpenAuth.AspNet.Clients;
using Laser.Orchard.OpenAuthentication.Models;
using Laser.Orchard.OpenAuthentication.Security;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;


namespace Laser.Orchard.OpenAuthentication.Services.Clients {
    public class GoogleOpenIdAuthenticationClient : IExternalAuthenticationClient {
        
        public string ProviderName {
            get { return "Google"; }
        }

        private const string UserInfoEndpoint = "https://www.googleapis.com/oauth2/v1/userinfo";


        public IAuthenticationClient Build(ProviderConfigurationRecord providerConfigurationRecord) {

            string ClientId = providerConfigurationRecord.ProviderIdKey;
            string ClientSecret = providerConfigurationRecord.ProviderSecret;
            
            var client = new GoogleOAuth2Client(ClientId, ClientSecret);

            return client;
        }


       

       /// <summary>
       /// 
       /// </summary>
       /// <param name="createUserParams"></param>
       /// <returns></returns>
        public OpenAuthCreateUserParams NormalizeData(OpenAuthCreateUserParams createUserParams) 
        {
            OpenAuthCreateUserParams retVal;

            retVal = createUserParams;
            string emailAddress = string.Empty;
           
            var valoriRicavati = createUserParams.ExtraData.Values;
            int countVal = 0;

            foreach (string valric in valoriRicavati) 
            {
                if (countVal == 1) {
                    emailAddress = valric;
                    retVal.UserName = emailAddress;
                }

                countVal = countVal + 1;
            }

            return retVal;
       
       }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientConfiguration"></param>
        /// <param name="userAccessToken"></param>
        /// <param name="userAccessSecret"></param>
        /// <returns></returns>
        public AuthenticationResult GetUserData(ProviderConfigurationRecord clientConfiguration, string userAccessToken, string userAccessSecret = "") {
            var userData = new Dictionary<string, string>();

            userData = GetUserDataGoogle(userAccessToken);
            userData["accesstoken"] = userAccessToken;

            string id = userData["client_id"];
            string name = "";

            return new AuthenticationResult(
                isSuccessful: true, provider: this.ProviderName, providerUserId: id, userName: name, extraData: userData);

        }

       
        /// <summary>
       /// 
       /// </summary>
       /// <param name="userAccessToken"></param>
       /// <returns></returns>
       public Dictionary<string, string> GetUserDataGoogle(string userAccessToken)
       {
           var uri = GoogleOAuth2Client.BuildUri(UserInfoEndpoint, new NameValueCollection { { "access_token", userAccessToken } });

            var webRequest = (HttpWebRequest)WebRequest.Create(uri);

            using (var webResponse = webRequest.GetResponse())
            using (var stream = webResponse.GetResponseStream()) {
                if (stream == null)
                    return null;

                using (var textReader = new StreamReader(stream)) {
                    var json = textReader.ReadToEnd();
                    var extraData = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    return extraData;
                }
            }
       }


    }
}