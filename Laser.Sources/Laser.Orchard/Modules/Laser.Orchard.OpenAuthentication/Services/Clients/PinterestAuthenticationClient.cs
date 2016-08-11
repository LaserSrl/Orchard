using DotNetOpenAuth.AspNet;
using Laser.Orchard.OpenAuthentication.Models;
using Laser.Orchard.OpenAuthentication.Security;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace Laser.Orchard.OpenAuthentication.Services.Clients {
    public class PinterestAuthenticationClient : IExternalAuthenticationClient {
        public string ProviderName {
            get { return "Pinterest"; }
        }

        private const string UserInfoEndpoint = "https://api.pinterest.com/v1/me/";

        public IAuthenticationClient Build(ProviderConfigurationRecord providerConfigurationRecord) {
            string ClientId = providerConfigurationRecord.ProviderIdKey;
            string ClientSecret = providerConfigurationRecord.ProviderSecret;
            var client = new PinterestOAuth2Client(ClientId, ClientSecret);
            return client;
        }

        public AuthenticationResult GetUserData(ProviderConfigurationRecord clientConfiguration, AuthenticationResult previousAuthResult, string userAccessToken, string userAccessSecretKey = "") {
            var userData = new Dictionary<string, string>();
            userData = GetUserDataPinterest(userAccessToken);
            userData["accesstoken"] = userAccessToken;
            string id = userData["id"];
            string name = userData["first_name"] + userData["last_name"];
            return new AuthenticationResult(true, this.ProviderName, id, name, userData);
        }

        private Dictionary<string, string> GetUserDataPinterest(string userAccessToken) {
            var uri = GoogleOAuth2Client.BuildUri(UserInfoEndpoint, new NameValueCollection { { "access_token", userAccessToken } });
            var webRequest = (HttpWebRequest)WebRequest.Create(uri);

            using (var webResponse = webRequest.GetResponse())
            using (var stream = webResponse.GetResponseStream()) {
                if (stream == null)
                    return null;

                using (var textReader = new StreamReader(stream)) {
                    var json = textReader.ReadToEnd();
                    var valori = JsonConvert.DeserializeObject<PinterestUserData>(json);
                    var extraData = valori.data;
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
            if (stateString != null && stateString.Contains("__provider__=Pinterest")) {
                // LinkedIn requires that all return data be packed into a "state" parameter
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