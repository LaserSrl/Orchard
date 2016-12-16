using Laser.Orchard.HID.Models;
using Laser.Orchard.HID.Extensions;
using Orchard;
using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using Orchard.Caching.Services;
using Orchard.Environment.Configuration;
using Orchard.Security;

namespace Laser.Orchard.HID.Services {
    public class HIDService : IHIDAdminService, IHIDAPIService {

        private readonly IOrchardServices _orchardServices;
        private readonly ICacheStorageProvider _cacheStorageProvider;
        private readonly ShellSettings _shellSetting;

        public HIDService(IOrchardServices orchardServices,
            ICacheStorageProvider cacheStorageProvider,
            ShellSettings shellSetting) {
            _orchardServices = orchardServices;
            _cacheStorageProvider = cacheStorageProvider;
            _shellSetting = shellSetting;
        }

        private string BaseURI {
            get { return String.Format(HIDAPIEndpoints.BaseURIFormat, GetSiteSettings().UseTestEnvironment ? HIDAPIEndpoints.BaseURITest : HIDAPIEndpoints.BaseURIProd); }
        }
        private string BaseEndpoint {
            get { return String.Format(HIDAPIEndpoints.CustomerURIFormat, BaseURI, GetSiteSettings().CustomerID.ToString()); }
        }
        private string UsersEndpoint {
            get { return String.Format(HIDAPIEndpoints.UsersEndpointFormat, BaseEndpoint); }
        }
        private string UsersSearchEndpoint {
            get { return String.Format(HIDAPIEndpoints.UserSearchEndpointFormat, UsersEndpoint); }
        }
        private string CreateInvitationEndpointFormat {
            get { return string.Format(HIDAPIEndpoints.CreateInvitationEndpointFormat, UsersEndpoint, @"{0}"); }
        }
        private string IssueCredentialEndpointFormat {
            get { return string.Format(HIDAPIEndpoints.IssueCredentialEndpointFormat, BaseEndpoint, @"{0}"); }
        }
        private string RevokeCredentialEndpointFormat {
            get { return string.Format(HIDAPIEndpoints.RevokeCredentialEndpointFormat, BaseEndpoint, @"{0}"); }
        }
        #region Token is in the cache
        private string CacheTokenTypeKey {
            get { return string.Format(Constants.CacheTokenTypeKeyFormat, _shellSetting.Name); }
        }
        private string CacheAccessTokenKey {
            get { return string.Format(Constants.CacheAccessTokenKeyFormat, _shellSetting.Name); }
        }
        private void AuthTokenToCache(HIDAuthToken token) {
            TimeSpan validity = TimeSpan.FromSeconds(token.ExpiresIn * 0.8); //80% of what is given by the authentication, to have margin
            _cacheStorageProvider.Put(CacheTokenTypeKey, token.TokenType, validity);
            _cacheStorageProvider.Put(CacheAccessTokenKey, token.AccessToken, validity);
        }
        #endregion
        public string AuthorizationToken {
            get {
                string tokenType = (string)_cacheStorageProvider.Get(CacheTokenTypeKey);
                string accessToken = (string)_cacheStorageProvider.Get(CacheAccessTokenKey);
                if (!string.IsNullOrWhiteSpace(tokenType) && !string.IsNullOrWhiteSpace(accessToken)) {
                    return tokenType + " " + accessToken;
                }
                return ""; //TODO: regenerate token here
            }
        }

        public HIDSiteSettingsPart GetSiteSettings() {
            return _orchardServices.WorkContext.CurrentSite.As<HIDSiteSettingsPart>();
        }

        public AuthenticationErrors Authenticate() {
            var token = HIDAuthToken.Authenticate(this);
            switch (token.Error) {
                case AuthenticationErrors.NoError:
                    AuthTokenToCache(token);
                    break;
                case AuthenticationErrors.NotAuthenticated:
                    break;
                case AuthenticationErrors.ClientInfoInvalid:
                    break;
                case AuthenticationErrors.CommunicationError:
                    break;
                default:
                    break;
            }
            return token.Error;
        }

        private const string BaseSearchFormat = @"{{ 'schemas':[ 'urn:ietf:params:scim:api:messages:2.0:SearchRequest' ], 'filter':'externalId eq ""{0}""', 'sortBy':'name.familyName', 'sortOrder':'descending', 'startIndex':1, 'count':{1} }}";
        private string CreateSearchFormat(string eId, int count = 1) {
            JObject format = JObject.Parse(string.Format(BaseSearchFormat, eId, count.ToString()));
            return format.ToString();
        }

        public HIDUser SearchHIDUser(IUser user) {
            return SearchHIDUserByExternalID(user.Id.ToString());
        }
        public HIDUser SearchHIDUserByExternalID(string externalId) {
            if (string.IsNullOrWhiteSpace(AuthorizationToken)) {
                if (Authenticate() != AuthenticationErrors.NoError) {
                    return null;
                }
            }
            HttpWebRequest wr = HttpWebRequest.CreateHttp(UsersSearchEndpoint);
            wr.Method = WebRequestMethods.Http.Post;
            wr.ContentType = "application/vnd.assaabloy.ma.credential-management-1.0+json";
            wr.Headers.Add(HttpRequestHeader.Authorization, AuthorizationToken);
            string bodyText = CreateSearchFormat(externalId); //("j.gerbore"); // 
            byte[] bodyData = Encoding.UTF8.GetBytes(bodyText);
            using (Stream reqStream = wr.GetRequestStream()) {
                reqStream.Write(bodyData, 0, bodyData.Length);
            }
            try {
                using (HttpWebResponse resp = wr.GetResponse() as HttpWebResponse) {
                    if (resp.StatusCode == HttpStatusCode.OK) {
                        //read the json response
                        using (var reader = new StreamReader(resp.GetResponseStream())) {
                            string respJson = reader.ReadToEnd();
                            var jo = JObject.Parse(respJson);
                            int nResults = int.Parse(jo["totalResults"].ToString());
                            if (nResults == 1) {
                                return HIDUser.GetUser(this, jo["Resources"].Children().First()["meta"]["location"].ToString());
                            } else if (nResults == 0) { //TODO: handle error cases
                                return null;
                            } else {
                                return null;
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                HttpWebResponse resp = (System.Net.HttpWebResponse)((System.Net.WebException)ex).Response;
                if (resp != null) {
                    switch (resp.StatusCode) {
                        case HttpStatusCode.BadRequest:

                            break;
                        case HttpStatusCode.Unauthorized:
                            //TODO: do login and try again
                            break;
                        default:
                            if (resp.StatusDescription.ToUpperInvariant() == "SERVER ERROR") {

                            } else {

                            }
                            break;
                    }
                } else {

                }
            }

            return null;
        }




    }


}