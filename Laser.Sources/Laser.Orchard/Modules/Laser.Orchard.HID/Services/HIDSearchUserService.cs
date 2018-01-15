using Laser.Orchard.HID.Extensions;
using Laser.Orchard.HID.Models;
using Newtonsoft.Json.Linq;
using Orchard.Logging;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Laser.Orchard.HID.Services {
    public class HIDSearchUserService : IHIDSearchUserService {
        
        private readonly IHIDAdminService _HIDAdminService;

        public ILogger Logger { get; set; }

        public HIDSearchUserService(
            IHIDAdminService HIDAdminService) {

            _HIDAdminService = HIDAdminService;
        }
        public string UsersEndpoint {
            get { return _HIDAdminService.UsersEndpoint; }
        }

        private string UsersSearchEndpoint {
            get { return String.Format(HIDAPIEndpoints.UserSearchEndpointFormat, UsersEndpoint); }
        }

        private bool VerifyAuthentication() {
            return _HIDAdminService.VerifyAuthentication();
        }

        private string AuthorizationToken {
            get { return _HIDAdminService.AuthorizationToken; }
        }

        private const string BaseSearchByMailFormat = @"{{ 'schemas':[ 'urn:ietf:params:scim:api:messages:2.0:SearchRequest' ], 'filter':'emails eq ""{0}"" and status eq ""ACTIVE""', 'sortBy':'name.familyName', 'sortOrder':'descending', 'startIndex':1, 'count':{1} }}";

        /// <summary>
        /// Creates a user's search string by email.
        /// </summary>
        /// <param name="email">The email to search for.</param>
        /// <param name="count">The number of users to return from the search.</param>
        /// <returns>A string that may be used as body in search requests to HID's systems.</returns>
        private string CreateSearchFormatByMail(string email, int count = 20) {
            JObject format = JObject.Parse(string.Format(BaseSearchByMailFormat, email.ToLowerInvariant(), count.ToString()));
            return format.ToString();
        }

        private AuthenticationErrors Authenticate() {
            return _HIDAdminService.Authenticate();
        }

        public HIDUserSearchResult SearchHIDUser(string email) {
            HIDUserSearchResult result = new HIDUserSearchResult();
            if (!VerifyAuthentication()) {
                result.Error = SearchErrors.AuthorizationFailed;
                return result;
            }

            HttpWebRequest wr = HttpWebRequest.CreateHttp(UsersSearchEndpoint);
            wr.Method = WebRequestMethods.Http.Post;
            wr.ContentType = Constants.DefaultContentType;
            wr.Headers.Add(HttpRequestHeader.Authorization, AuthorizationToken);
            string bodyText = CreateSearchFormatByMail(email);
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
                                result = SearchHIDUser(email);
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
    }
}