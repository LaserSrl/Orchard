using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using DotNetOpenAuth.AspNet;
using DotNetOpenAuth.AspNet.Clients;
using Laser.Orchard.OpenAuthentication.Models;
using Laser.Orchard.OpenAuthentication.Security;

namespace Laser.Orchard.OpenAuthentication.Services.Clients {
    public class TwitterAuthenticationClient : IExternalAuthenticationClient {
        public string ProviderName {
            get { return "Twitter"; }
        }

        public IAuthenticationClient Build(ProviderConfigurationRecord providerConfigurationRecord) {
           
            return new TwitterClient(providerConfigurationRecord.ProviderIdKey, providerConfigurationRecord.ProviderSecret);
        }


        public AuthenticationResult GetUserData(ProviderConfigurationRecord clientConfiguration, AuthenticationResult previousAuthResult, string userAccessToken, string userAccessSecret = "") {
            if (String.IsNullOrWhiteSpace(userAccessSecret)) {
                if (previousAuthResult.ExtraData.ContainsKey("accesstoken") == false) {
                    previousAuthResult.ExtraData.Add("accesstoken", userAccessToken);
                }
                return new AuthenticationResult(true, this.ProviderName, previousAuthResult.ProviderUserId, previousAuthResult.UserName, previousAuthResult.ExtraData);
            }
            var twitterUserSerializer = new DataContractJsonSerializer(typeof(TwitterUserData));

            TwitterUserData twitterUserData;
            // recupero lo User_Name relativo al token 
            var accountSettingsRequest = PrepareAuthorizedRequest(userAccessToken,
                userAccessSecret,
                clientConfiguration.ProviderIdKey,
                clientConfiguration.ProviderSecret,
                "https://api.twitter.com/1.1/account/settings.json",
                "GET");
            try {
                using (var response = accountSettingsRequest.GetResponse()) {
                    using (var responseStream = response.GetResponseStream()) {
                        twitterUserData = (TwitterUserData)twitterUserSerializer.ReadObject(responseStream);
                        if (String.IsNullOrWhiteSpace(twitterUserData.Screen_Name)) {
                            return AuthenticationResult.Failed;
                        }
                    }
                }
            } catch {
                return AuthenticationResult.Failed;
            }

            var userData = new Dictionary<string, string>();
            userData["id"] = userAccessToken.Split('-')[0];
            userData["username"] = twitterUserData.Screen_Name;
            userData["email"] = twitterUserData.Email;

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


        private HttpWebRequest PrepareAuthorizedRequest(string oauth_token, string oauth_token_secret, string oauth_consumer_key, string oauth_consumer_secret, string resource_url, string httpMethod) {

            var oauth_version = "1.0";
            var oauth_signature_method = "HMAC-SHA1";
            var oauth_nonce = Convert.ToBase64String(
                                              new ASCIIEncoding().GetBytes(
                                                   DateTime.Now.Ticks.ToString()));
            var timeSpan = DateTime.UtcNow
                                              - new DateTime(1970, 1, 1, 0, 0, 0, 0,
                                                   DateTimeKind.Utc);
            var oauth_timestamp = Convert.ToInt64(timeSpan.TotalSeconds).ToString();
            var baseFormat = "oauth_consumer_key={0}&oauth_nonce={1}&oauth_signature_method={2}" +
                            "&oauth_timestamp={3}&oauth_token={4}&oauth_version={5}";

            var baseString = string.Format(baseFormat,
                                        oauth_consumer_key,
                                        oauth_nonce,
                                        oauth_signature_method,
                                        oauth_timestamp,
                                        oauth_token,
                                        oauth_version
                                        );
            Uri urlToCall = new Uri(resource_url);
            var normalizedResourceUrl = String.Format("{0}{1}{2}{3}", urlToCall.Scheme, Uri.SchemeDelimiter, urlToCall.Authority, urlToCall.AbsolutePath);
            string queryString = urlToCall.Query;
            baseString = string.Concat(httpMethod.ToUpper(), "&", Uri.EscapeDataString(normalizedResourceUrl),
                "&", Uri.EscapeDataString(baseString), String.IsNullOrWhiteSpace(queryString) ? "" : "&" + Uri.EscapeDataString(queryString));
            var compositeKey = string.Concat(Uri.EscapeDataString(oauth_consumer_secret),
                                    "&", Uri.EscapeDataString(oauth_token_secret));

            string oauth_signature;
            using (HMACSHA1 hasher = new HMACSHA1(ASCIIEncoding.ASCII.GetBytes(compositeKey))) {
                oauth_signature = Convert.ToBase64String(
                    hasher.ComputeHash(ASCIIEncoding.ASCII.GetBytes(baseString)));
            }
            var headerFormat = "OAuth oauth_nonce=\"{0}\", oauth_signature_method=\"{1}\", " +
                               "oauth_timestamp=\"{2}\", oauth_consumer_key=\"{3}\", " +
                               "oauth_token=\"{4}\", oauth_signature=\"{5}\", " +
                               "oauth_version=\"{6}\"";

            var authHeader = string.Format(headerFormat,
                                    Uri.EscapeDataString(oauth_nonce),
                                    Uri.EscapeDataString(oauth_signature_method),
                                    Uri.EscapeDataString(oauth_timestamp),
                                    Uri.EscapeDataString(oauth_consumer_key),
                                    Uri.EscapeDataString(oauth_token),
                                    Uri.EscapeDataString(oauth_signature),
                                    Uri.EscapeDataString(oauth_version)
                            );

            ServicePointManager.Expect100Continue = false;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(resource_url);
            request.Headers.Add("Authorization", authHeader);
            request.Method = httpMethod.ToUpper();
            return request;

        }

        public OpenAuthCreateUserParams NormalizeData(OpenAuthCreateUserParams createUserParams) {
            return createUserParams;
        }

        public bool RewriteRequest() {
            return false;
        }
    }
}