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
using Orchard.Logging;
using Orchard.Data;
using Laser.Orchard.HID.ViewModels;


namespace Laser.Orchard.HID.Services {
    public class HIDService : IHIDAPIService {

        private readonly IOrchardServices _orchardServices;
        private readonly ICacheStorageProvider _cacheStorageProvider;
        private readonly ShellSettings _shellSetting;
        private readonly IHIDPartNumbersService _HIDPartNumbersService;
        private readonly IHIDAdminService _HIDAdminService;
        private readonly IHIDCredentialsService _HIDCredentialsService;
        private readonly IHIDSearchUserService _HIDSearchUserService;

        public ILogger Logger { get; set; }

        public HIDService(
            IOrchardServices orchardServices,
            ICacheStorageProvider cacheStorageProvider,
            ShellSettings shellSetting,
            IHIDPartNumbersService HIDPartNumbersService,
            IHIDAdminService HIDAdminService,
            IHIDCredentialsService HIDCredentialsService,
            IHIDSearchUserService HIDSearchUserService) {

            _orchardServices = orchardServices;
            _cacheStorageProvider = cacheStorageProvider;
            _shellSetting = shellSetting;
            _HIDPartNumbersService = HIDPartNumbersService;
            _HIDAdminService = HIDAdminService;
            _HIDCredentialsService = HIDCredentialsService;
            _HIDSearchUserService = HIDSearchUserService;

            Logger = NullLogger.Instance;
        }
        
        public string UsersEndpoint {
            get { return _HIDAdminService.UsersEndpoint; }
        }

        private string UsersSearchEndpoint {
            get { return String.Format(HIDAPIEndpoints.UserSearchEndpointFormat, UsersEndpoint); }
        }
                
        private string AuthorizationToken {
            get {return _HIDAdminService.AuthorizationToken; }
        }
        
        private AuthenticationErrors Authenticate() {
            return _HIDAdminService.Authenticate();
        }

        public bool VerifyAuthentication() {
            return _HIDAdminService.VerifyAuthentication();
        }

        private const string BaseSearchFormat = @"{{ 'schemas':[ 'urn:ietf:params:scim:api:messages:2.0:SearchRequest' ], 'filter':'externalId eq ""{0}"" and status eq ""ACTIVE""', 'sortBy':'name.familyName', 'sortOrder':'descending', 'startIndex':1, 'count':{1} }}";

        /// <summary>
        /// Creates a user's search string by externalId.
        /// </summary>
        /// <param name="eId">The externalId to search for.</param>
        /// <param name="count">The number of users to return from the search.</param>
        /// <returns>A string that may be used as body in search requests to HID's systems.</returns>
        private string CreateSearchFormat(string eId, int count = 20) {
            JObject format = JObject.Parse(string.Format(BaseSearchFormat, eId, count.ToString()));
            return format.ToString();
        }

        public HIDUserSearchResult SearchHIDUser(IUser user) {
            return SearchHIDUserByExternalID(HIDUser.GenerateExternalId(user.Id));
        }
        public HIDUserSearchResult SearchHIDUserByExternalID(string externalId) {
            HIDUserSearchResult result = new HIDUserSearchResult();
            if (!VerifyAuthentication()) {
                result.Error = SearchErrors.AuthorizationFailed;
                return result;
            }

            HttpWebRequest wr = HttpWebRequest.CreateHttp(UsersSearchEndpoint);
            wr.Method = WebRequestMethods.Http.Post;
            wr.ContentType = Constants.DefaultContentType;
            wr.Headers.Add(HttpRequestHeader.Authorization, AuthorizationToken);
            string bodyText = CreateSearchFormat(externalId);
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
                                result.User = HIDUser.GetUser(_HIDAdminService, jo["Resources"].Children().First()["meta"]["location"].ToString());
                                result.Error = SearchErrors.NoError;
                            } else if (result.TotalResults == 0) {
                                result.Error = SearchErrors.NoResults;
                            } else {
                                result.Error = SearchErrors.TooManyResults;
                            }
                        }
                    }
                }
            } catch (WebException ex) {
                HttpWebResponse resp = (System.Net.HttpWebResponse)(ex.Response);
                if (resp != null) {
                    switch (resp.StatusCode) {
                        case HttpStatusCode.BadRequest:
                            result.Error = SearchErrors.InvalidParameters;
                            break;
                        case HttpStatusCode.Unauthorized:
                            // Authentication could have expired while this method was running
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
            } catch (Exception ex) {
                result.Error = SearchErrors.UnknownError;
                Logger.Error(ex, "Fallback error management.");
            }

            return result;
        }

        public HIDUserSearchResult SearchHIDUser(string email) {
            return _HIDSearchUserService.SearchHIDUser(email);
        }

        public HIDUser CreateHIDUser(IUser user, string familyName, string givenName, string email = null) {
            if (string.IsNullOrWhiteSpace(email)) {
                email = user.Email;
            }
            return HIDUser.CreateUser(_HIDAdminService, user, familyName, givenName, email);
        }

        public HIDUser IssueCredentials(IUser user) {
            return IssueCredentials(user, _HIDPartNumbersService.GetPartNumbersForUser(user));
        }

        public HIDUser IssueCredentials(HIDUser hidUser) {
            return IssueCredentials(hidUser, _HIDPartNumbersService.GetPartNumbersForUser(hidUser));
        }

        public HIDUser IssueCredentials(IUser user, string[] partNumbers) {
            return _HIDCredentialsService.IssueCredentials(user, partNumbers);
        }

        public HIDUser IssueCredentials(HIDUser hidUser, string[] partNumbers) {
            return _HIDCredentialsService.IssueCredentials(hidUser, partNumbers);
        }
        
        public HIDUser RevokeCredentials(IUser user) {
            return RevokeCredentials(user, _HIDPartNumbersService.GetPartNumbersForUser(user));
        }

        public HIDUser RevokeCredentials(HIDUser hidUser) {
            return RevokeCredentials(hidUser, _HIDPartNumbersService.GetPartNumbersForUser(hidUser));
        }

        public HIDUser RevokeCredentials(IUser user, string[] partNumbers) {
            return _HIDCredentialsService.RevokeCredentials(user, partNumbers);
        }

        public HIDUser RevokeCredentials(HIDUser hidUser, string[] partNumbers) {
            return _HIDCredentialsService.RevokeCredentials(hidUser, partNumbers);
        }
    }


}