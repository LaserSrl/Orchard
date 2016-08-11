using DotNetOpenAuth.AspNet;
using DotNetOpenAuth.AspNet.Clients;
using Laser.Orchard.OpenAuthentication.Models;
using Laser.Orchard.OpenAuthentication.Security;
using Laser.Orchard.OpenAuthentication.Extensions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Xml.Linq;
using System.Linq;
using System.Web;

namespace Laser.Orchard.OpenAuthentication.Services.Clients {
    public class LinkedInAuthenticationClient : IExternalAuthenticationClient {
        public string ProviderName {
            get { return "LinkedIn"; }
        }

        private const string UserInfoEndpoint = "https://api.linkedin.com/v1/people/~" + UserProfileFields;

        //private const string UserProfileFields = ":(id,first-name,last-name,headline,location:(name),industry,summary,picture-url,email-address,phone-numbers,main-address)";
        private const string UserProfileFields = ":(id,first-name,last-name,email-address)";

        public IAuthenticationClient Build(ProviderConfigurationRecord providerConfigurationRecord) {
            string ClientId = providerConfigurationRecord.ProviderIdKey;
            string ClientSecret = providerConfigurationRecord.ProviderSecret;

            var client = new LinkedInOAuth2Client(ClientId, ClientSecret);

            return client;
        }


        public AuthenticationResult GetUserData(ProviderConfigurationRecord clientConfiguration, AuthenticationResult previosAuthResult, string userAccessToken, string userAccessSecret = "") {
            var userData = new Dictionary<string, string>();

            userData = GetUserDataLinkedin(userAccessToken);
            userData["accesstoken"] = userAccessToken;

            string id = userData["id"];
            string name = userData["email-address"];
            userData["name"] = userData["email-address"];

            return new AuthenticationResult(
                isSuccessful: true, provider: this.ProviderName, providerUserId: id, userName: name, extraData: userData);
        }

        /// <summary>
        /// 
        /// 
        /// </summary>
        /// <param name="userAccessToken"></param>
        /// <returns></returns>
        public Dictionary<string, string> GetUserDataLinkedin(string userAccessToken) {
            var uri = LinkedInOAuth2Client.BuildUri(UserInfoEndpoint, new NameValueCollection { { "oauth2_access_token", userAccessToken } });

            var webRequest = (HttpWebRequest)WebRequest.Create(uri);

            using (var webResponse = webRequest.GetResponse())
            using (var stream = webResponse.GetResponseStream()) {
                if (stream == null)
                    return null;

                using (var textReader = new StreamReader(stream)) {
                    var xml = textReader.ReadToEnd();

                    var extraData = XElement.Parse(xml)
                    .Elements()
                    .ToDictionary(
                        el => el.Name.LocalName,
                        el => el.Value
                    );

                    extraData.Add("accesstoken", userAccessToken);

                    return extraData;
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="createUserParams"></param>
        /// <returns></returns>
        public OpenAuthCreateUserParams NormalizeData(OpenAuthCreateUserParams createUserParams) {
            OpenAuthCreateUserParams retVal = createUserParams;
            string emailAddress = string.Empty;
            foreach (KeyValuePair<string, string> values in createUserParams.ExtraData) {
                if (values.Key == "email-address") {
                    retVal.UserName = values.Value.IsEmailAddress() ? values.Value.Substring(0, values.Value.IndexOf('@')) : values.Value;
                }
            }
            return retVal;
        }

        public bool RewriteRequest() {
            bool result = false;
            var ctx = HttpContext.Current;
            var stateString = System.Web.HttpUtility.UrlDecode(ctx.Request.QueryString["state"]);
            if (stateString != null && stateString.Contains("__provider__=linkedin")) {
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