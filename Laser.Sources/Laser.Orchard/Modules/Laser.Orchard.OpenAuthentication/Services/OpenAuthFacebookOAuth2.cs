using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using DotNetOpenAuth.AspNet.Clients;
using Newtonsoft.Json;
using System.Runtime.Serialization.Json;
using Laser.Orchard.OpenAuthentication.Extensions; 

namespace Laser.Orchard.OpenAuthentication.Services {
    public class FacebookOAuth2Client :  OAuth2Client {

        #region Constants and Fields

        /// <summary>
        /// The authorization endpoint.
        /// </summary>
        private const string AuthorizationEndpoint = "https://www.facebook.com/dialog/oauth";

        /// <summary>
        /// The token endpoint.
        /// </summary>
        private const string TokenEndpoint = "https://graph.facebook.com/oauth/access_token";

        /// <summary>
        /// The user info endpoint.
        /// </summary>
        private const string UserInfoEndpoint = "https://graph.facebook.com/me";

        /// <summary>
        /// The app id.
        /// </summary>
        private readonly string _appId;

        /// <summary>
        /// The app secret.
        /// </summary>
        private readonly string _appSecret;

        /// <summary>
        /// The requested scopes.
        /// </summary>
        private readonly string[] _requestedScopes;

        #endregion

        /// <summary>
        /// Creates a new Facebook OAuth2 client, requesting the default "email" scope.
        /// </summary>
        /// <param name="appId">The Facebook App Id</param>
        /// <param name="appSecret">The Facebook App Secret</param>
        public FacebookOAuth2Client(string appId, string appSecret)
            : this(appId, appSecret, new[] { "email" }) { }

        /// <summary>
        /// Creates a new Facebook OAuth2 client.
        /// </summary>
        /// <param name="appId">The Facebook App Id</param>
        /// <param name="appSecret">The Facebook App Secret</param>
        /// <param name="requestedScopes">One or more requested scopes, passed without the base URI.</param>
        public FacebookOAuth2Client(string appId, string appSecret, params string[] requestedScopes)
            : base("facebook")
        {
            if (string.IsNullOrWhiteSpace(appId))
                throw new ArgumentNullException("appId");

            if (string.IsNullOrWhiteSpace(appSecret))
                throw new ArgumentNullException("appSecret");

            if (requestedScopes == null)
                throw new ArgumentNullException("requestedScopes");

            if (requestedScopes.Length == 0)
                throw new ArgumentException("One or more scopes must be requested.", "requestedScopes");

            _appId = appId;
            _appSecret = appSecret;
            _requestedScopes = requestedScopes;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        protected override Uri GetServiceLoginUrl(Uri returnUrl)
        {
            var state = string.IsNullOrEmpty(returnUrl.Query) ? string.Empty : returnUrl.Query.Substring(1);

            return BuildUri(AuthorizationEndpoint, new NameValueCollection
                {
                    { "client_id", _appId },
                    { "scope", string.Join(" ", _requestedScopes) },
                    { "redirect_uri", returnUrl.GetLeftPart(UriPartial.Path) },
                    { "state", state },
                });
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        protected override IDictionary<string, string> GetUserData(string accessToken)
        {
           
            var serializer = new DataContractJsonSerializer(typeof(FacebookGraphData));
            FacebookGraphData graphData;
            var request =
                WebRequest.Create(
                    "https://graph.facebook.com/me?fields=id,email,birthday,first_name,last_name,name,locale,link,gender,timezone,updated_time,verified&access_token=" + accessToken);
            try {
                using (var response = request.GetResponse()) {
                    using (var responseStream = response.GetResponseStream()) {

                        graphData = (FacebookGraphData)serializer.ReadObject(responseStream);
                    }
                }
            } catch {
               // return AuthenticationResult.Failed;
                return null;
            }
            // this dictionary must contains 
            var userData = new Dictionary<string, string>();
            userData["id"] = graphData.Id;
            userData["username"] = graphData.Email.IsEmailAddress() ? graphData.Email.Substring(0, graphData.Email.IndexOf('@')) : graphData.Email; 
            userData["name"] = graphData.Name;
            userData["link"] = graphData.Link == null ? null : graphData.Link.AbsoluteUri;
            userData["gender"] = graphData.Gender;
            userData["birthday"] = graphData.Birthday;
            userData["email"] = graphData.Email;

            if (userData == null) {
                //return AuthenticationResult.Failed;
                return null;
            }

            string id = userData["id"];
            string name;

            // Some oAuth providers do not return value for the 'username' attribute. 
            // In that case, try the 'name' attribute. If it's still unavailable, fall back to 'id'
            if (!userData.TryGetValue("username", out name) && !userData.TryGetValue("name", out name)) {
                name = id;
            }

            // add the access token to the user data dictionary just in case page developers want to use it
            userData["accesstoken"] = accessToken;

            return userData;

        }



        protected override string QueryAccessToken(Uri returnUrl, string authorizationCode)
        {
            var uri = BuildUri(TokenEndpoint, new NameValueCollection
                {
                    { "code", authorizationCode },
                    { "client_id", _appId },
                    { "client_secret", _appSecret },
                    { "redirect_uri", returnUrl.GetLeftPart(UriPartial.Path) },
                });

            var webRequest = (HttpWebRequest) WebRequest.Create(uri);

            using (var webResponse = webRequest.GetResponse())
            {
                var responseStream = webResponse.GetResponseStream();
                if (responseStream == null)
                    return null;

                using (var reader = new StreamReader(responseStream))
                {
                    var response = reader.ReadToEnd();

                    var results = HttpUtility.ParseQueryString(response);
                    return results["access_token"];
                }
            }
        }

        private static Uri BuildUri(string baseUri, NameValueCollection queryParameters)
        {
            var keyValuePairs = queryParameters.AllKeys.Select(k => HttpUtility.UrlEncode(k) + "=" + HttpUtility.UrlEncode(queryParameters[k]));
            var qs = String.Join("&", keyValuePairs);

            var builder = new UriBuilder(baseUri) { Query = qs };
            return builder.Uri;
        }

        /// <summary>
        /// Facebook works best when return data be packed into a "state" parameter.
        /// This should be called before verifying the request, so that the url is rewritten to support this.
        /// </summary>
        public static void RewriteRequest()
        {
            var ctx = HttpContext.Current;

            var stateString = HttpUtility.UrlDecode(ctx.Request.QueryString["state"]);
            if (stateString == null || !stateString.Contains("__provider__=facebook"))
                return;

            var q = HttpUtility.ParseQueryString(stateString);
            q.Add(ctx.Request.QueryString);
            q.Remove("state");

            ctx.RewritePath(ctx.Request.Path + "?" + q);
        }
    }
}