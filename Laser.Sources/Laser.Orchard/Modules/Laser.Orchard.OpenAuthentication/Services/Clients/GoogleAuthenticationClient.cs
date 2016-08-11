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
    public class GoogleAuthenticationClient : IExternalAuthenticationClient {

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

        public OpenAuthCreateUserParams NormalizeData(OpenAuthCreateUserParams createUserParams) {
            OpenAuthCreateUserParams retVal = createUserParams;
            string emailAddress = string.Empty;
            foreach (KeyValuePair<string, string> values in createUserParams.ExtraData) {
                if (values.Key == "mail") {
                    retVal.UserName = values.Value.IsEmailAddress() ? values.Value.Substring(0, values.Value.IndexOf('@')) : values.Value;
                }
            }
            return retVal;
        }


        public AuthenticationResult GetUserData(ProviderConfigurationRecord clientConfiguration, AuthenticationResult previosAuthResult, string userAccessToken, string userAccessSecret = "") {
            var userData = new Dictionary<string, string>();
            userData = GetUserDataGoogle(userAccessToken);
            userData["accesstoken"] = userAccessToken;
            string id = userData["id"];
            string name = userData["email"];
            userData["name"] = userData["email"];
            return new AuthenticationResult(true, this.ProviderName, id, name, userData);
        }
        
        private Dictionary<string, string> GetUserDataGoogle(string userAccessToken) {
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

        public bool RewriteRequest() {
            bool result = false;
            var ctx = HttpContext.Current;
            var stateString = HttpUtility.UrlDecode(ctx.Request.QueryString["state"]);
            if (stateString != null && stateString.Contains("__provider__=google")) {
                // Google requires that all return data be packed into a "state" parameter
                var q = HttpUtility.ParseQueryString(stateString);
                q.Add(ctx.Request.QueryString);
                q.Remove("state");
                ctx.RewritePath(ctx.Request.Path + "?" + q.ToString());
                result = true;
            }
            return result;
        }
    }
}