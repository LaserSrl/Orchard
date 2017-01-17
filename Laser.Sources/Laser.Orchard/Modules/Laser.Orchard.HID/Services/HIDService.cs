using Laser.Orchard.HID.Models;
using Laser.Orchard.HID.Extensions;
using Orchard;
using Orchard.ContentManagement;
using System;
using System.IO;
using System.Linq;
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
        public string BaseEndpoint {
            get { return String.Format(HIDAPIEndpoints.CustomerURIFormat, BaseURI, GetSiteSettings().CustomerID.ToString()); }
        }
        public string UsersEndpoint {
            get { return String.Format(HIDAPIEndpoints.UsersEndpointFormat, BaseEndpoint); }
        }
        private string UsersSearchEndpoint {
            get { return String.Format(HIDAPIEndpoints.UserSearchEndpointFormat, UsersEndpoint); }
        }
        private string CreateInvitationEndpointFormat {
            get { return string.Format(HIDAPIEndpoints.CreateInvitationEndpointFormat, UsersEndpoint, @"{0}"); }
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

        public HIDUserSearchResult SearchHIDUser(IUser user) {
            return SearchHIDUserByExternalID(HIDUser.GenerateExternalId(user.Id));
        }
        public HIDUserSearchResult SearchHIDUserByExternalID(string externalId) {
            HIDUserSearchResult result = new HIDUserSearchResult();
            if (string.IsNullOrWhiteSpace(AuthorizationToken)) {
                if (Authenticate() != AuthenticationErrors.NoError) {
                    result.Error = SearchErrors.AuthorizationFailed;
                    return result;
                }
            }
            HttpWebRequest wr = HttpWebRequest.CreateHttp(UsersSearchEndpoint);
            wr.Method = WebRequestMethods.Http.Post;
            wr.ContentType = "application/vnd.assaabloy.ma.credential-management-1.0+json";
            wr.Headers.Add(HttpRequestHeader.Authorization, AuthorizationToken);
            string bodyText = CreateSearchFormat(externalId); // ("Laser.Orchard.HID1331"); //("m.piovanelli"); //
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
                            result = new HIDUserSearchResult(jo);
                            if (result.TotalResults == 1) {
                                result.User = HIDUser.GetUser(this, jo["Resources"].Children().First()["meta"]["location"].ToString());
                                result.Error = SearchErrors.NoError;
                            } else if (result.TotalResults == 0) {
                                result.Error = SearchErrors.NoResults;
                            } else {
                                result.Error = SearchErrors.TooManyResults;
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                HttpWebResponse resp = (System.Net.HttpWebResponse)((System.Net.WebException)ex).Response;
                if (resp != null) {
                    switch (resp.StatusCode) {
                        case HttpStatusCode.BadRequest:
                            result.Error = SearchErrors.InvalidParameters;
                            break;
                        case HttpStatusCode.Unauthorized:
                            if (Authenticate() == AuthenticationErrors.NoError) {
                                result = SearchHIDUserByExternalID(externalId);
                            } else {
                                result.Error = SearchErrors.AuthorizationFailed;
                            }
                            break;
                        case HttpStatusCode.InternalServerError:
                            result.Error = SearchErrors.InternalServerError;
                            break;
                        default:
                            result.Error = SearchErrors.UnknownError;
                            break;
                    }
                } else {
                    result.Error = SearchErrors.UnknownError;
                }
            }

            return result;
        }

        public HIDUser CreateHIDUser(IUser user, string familyName, string givenName, string email = null) {
            if (string.IsNullOrWhiteSpace(email)) {
                email = user.Email;
            }
            return HIDUser.CreateUser(this, user, familyName, givenName, email);
        }

        public HIDUser IssueCredentials(IUser user) {
            return IssueCredentials(user, GetSiteSettings().PartNumbers);
        }
        public HIDUser IssueCredentials(IUser user, string[] partNumbers) {
            var searchResult = SearchHIDUser(user);
            if (searchResult.Error == SearchErrors.NoError) {
                HIDUser hidUser = searchResult.User;
                if (partNumbers.Length == 0) {
                    hidUser = hidUser.IssueCredential(""); //this assigns the default part number for the customer
                } else {
                    foreach (var pn in partNumbers) {
                        hidUser = hidUser.IssueCredential(pn);
                        if (hidUser.Error != UserErrors.NoError && hidUser.Error != UserErrors.PreconditionFailed) {
                            break;  //break on error, but not on PreconditionFailed, because that may be caused by the credential having been
                                    //assigned already, which is fine
                        }
                    }
                }
                return hidUser;
            } else {
                return new HIDUser();
            }
        }

        public HIDUser RevokeCredentials(IUser user) {
            return RevokeCredentials(user, GetSiteSettings().PartNumbers);
        }
        public HIDUser RevokeCredentials(IUser user, string[] partNumbers) {
            var searchResult = SearchHIDUser(user);
            if (searchResult.Error == SearchErrors.NoError) {
                HIDUser hidUser = searchResult.User;
                if (partNumbers.Length == 0) {
                    hidUser = hidUser.RevokeCredential();
                } else {
                    foreach (var pn in partNumbers) {
                        hidUser = hidUser.RevokeCredential(pn);
                        if (hidUser.Error != UserErrors.NoError && hidUser.Error != UserErrors.PreconditionFailed) {
                            break;  //break on error, but not on PreconditionFailed, because that may be caused by the credential being
                                    //revoked right now
                        }
                    }
                }
                return hidUser;
            } else {
                return new HIDUser();
            }
        }
    }


}