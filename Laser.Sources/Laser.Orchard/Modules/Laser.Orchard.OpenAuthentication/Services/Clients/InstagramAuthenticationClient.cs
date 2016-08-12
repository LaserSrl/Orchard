using DotNetOpenAuth.AspNet;
using Laser.Orchard.OpenAuthentication.Models;
using Laser.Orchard.OpenAuthentication.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace Laser.Orchard.OpenAuthentication.Services.Clients {
    public class InstagramAuthenticationClient : IExternalAuthenticationClient {
        public string ProviderName {
            get { return "Instagram"; }
        }

        private const string UserInfoEndpoint = "https://api.instagram.com/v1/users/self/";

        public IAuthenticationClient Build(ProviderConfigurationRecord providerConfigurationRecord) {
            string ClientId = providerConfigurationRecord.ProviderIdKey;
            string ClientSecret = providerConfigurationRecord.ProviderSecret;
            var client = new InstagramOAuth2Client(ClientId, ClientSecret);
            return client;
        }

        public AuthenticationResult GetUserData(ProviderConfigurationRecord clientConfiguration, AuthenticationResult previousAuthResult, string userAccessToken, string userAccessSecretKey = "") {
            var userData = new Dictionary<string, string>();
            userData = GetUserDataInstagram(userAccessToken);
            userData["accesstoken"] = userAccessToken;
            string id = userData["id"];
            string username = userData["username"];
            return new AuthenticationResult(true, this.ProviderName, id, username, userData);
        }

        private Dictionary<string, string> GetUserDataInstagram(string userAccessToken) {
            var uri = InstagramOAuth2Client.BuildUri(UserInfoEndpoint, new NameValueCollection { { "access_token", userAccessToken } });
            var webRequest = (HttpWebRequest)WebRequest.Create(uri);

            using (var webResponse = webRequest.GetResponse())
            using (var stream = webResponse.GetResponseStream()) {
                if (stream == null)
                    return null;

                using (var textReader = new StreamReader(stream)) {
                    var json = textReader.ReadToEnd();
                    var valori = JObject.Parse(json);
                    var data = valori.SelectToken("data");
                    Dictionary<string, string> extraData = new Dictionary<string, string>();
                    extraData.Add("id", data.Value<string>("id"));
                    extraData.Add("username", data.Value<string>("username"));
                    extraData.Add("full_name", data.Value<string>("full_name"));
                    extraData.Add("profile_picture", data.Value<string>("profile_picture"));
                    return extraData;
                }
            }
        }

        public OpenAuthCreateUserParams NormalizeData(OpenAuthCreateUserParams clientData) {
            return clientData;
        }

        public bool RewriteRequest() {
            bool result = false;
            var ctx = HttpContext.Current;
            var stateString = System.Web.HttpUtility.UrlDecode(ctx.Request.QueryString["state"]);
            if (stateString != null && stateString.Contains("__provider__=Instagram")) {
                // Instagram requires that all return data be packed into a "state" parameter
                var q = System.Web.HttpUtility.ParseQueryString(stateString);
                q.Add(ctx.Request.QueryString);
                q.Remove("state");
                ctx.RewritePath(ctx.Request.Path + "?" + q.ToString());
                result = true;
            }
            return result;
        }
    }
}