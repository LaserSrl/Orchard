using Laser.Orchard.Caligoo.DataContracts;
using Laser.Orchard.Caligoo.Models;
using Laser.Orchard.CommunicationGateway.Services;
using Newtonsoft.Json.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Security;
using Orchard.Users.Models;
using Orchard.Users.Services;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Laser.Orchard.Caligoo.Services {
    public interface ICaligooService : IDependency {
        ContentItem GetContact(string caligooUserId);
        ContentItem CreateContact(LoginLogoutEventMessage caligooUserEvent);
        UserMessage GetUserDetails(string caligooUserId);
        List<UserMessage> GetFilteredCaligooUsers(CaligooUsersFilterValue filters);
        List<string> GetFilteredCaligooUsersIds(CaligooUsersFilterValue filters);
        List<LocationMessage> GetLocations();
        void UpdateLocation(LocationMessage messsage);
    }
    public class CaligooService : ICaligooService {
        private readonly IOrchardServices _orchardServices;
        private readonly ICommunicationService _communicationService;
        private readonly CaligooTempData _caligooTempData;
        private CaligooSiteSettingsPart _caligooSettings;
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }
        public CaligooService(IOrchardServices orchardServices, ICommunicationService communicationService, IUserService userService, CaligooTempData caligooTempData) {
            _orchardServices = orchardServices;
            _communicationService = communicationService;
            _caligooTempData = caligooTempData;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
            _caligooSettings = _orchardServices.WorkContext.CurrentSite.As<CaligooSiteSettingsPart>();
        }
        public ContentItem GetContact(string caligooUserId) {
            var query =_orchardServices.ContentManager.Query().ForType("CommunicationContact").Where<CaligooUserPartRecord>(x => x.CaligooUserId == caligooUserId);
            var result = query.List().FirstOrDefault();
            return result;
        }
        public ContentItem CreateContact(LoginLogoutEventMessage caligooUserEvent) {
            var contact = _orchardServices.ContentManager.New("CommunicationContact");
            contact.As<TitlePart>().Title = caligooUserEvent.CaligooUserName;
            var caligooUserPart = contact.As<CaligooUserPart>();
            caligooUserPart.CaligooUserId = caligooUserEvent.CaligooUserId;
            caligooUserPart.CaligooUserName = caligooUserEvent.CaligooUserName;
            var commonPart = contact.As<CommonPart>();
            if(commonPart != null) {
                commonPart.Owner = GetAdministrator();
            }
            _orchardServices.ContentManager.Create(contact);
            // if we will have email and phone number we can use the following code
            //if (string.IsNullOrWhiteSpace(email) == false) {
            //    _communicationService.AddEmailToContact(email, contact);
            //}
            //if (string.IsNullOrWhiteSpace(phone) == false) {
            //    _communicationService.AddSmsToContact("", phone, contact, false);
            //}
            return contact;
        }
        private IUser GetAdministrator() {
            if(_caligooTempData.KrakeAdmin == null) {
                var superUser = _orchardServices.WorkContext.CurrentSite.SuperUser;
                var query = _orchardServices.ContentManager.Query().ForType("User").Where<UserPartRecord>(x => x.UserName == superUser);
                var user = query.List().FirstOrDefault();
                if (user != null) {
                    _caligooTempData.KrakeAdmin = user.As<UserPart>();
                }
            }
            return _caligooTempData.KrakeAdmin;
        }
        private void CaligooLogin() {
            var url = _caligooSettings.LoginUrl;
            // basic authentication header
            var byteArr = Encoding.ASCII.GetBytes(string.Format("{0}:{1}", _caligooSettings.Username, _caligooSettings.Password));
            var authHeader = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArr));
            // call web api
            var response = CallWebApi(url, authHeader, HttpMethod.Get);
            var json = JObject.Parse(response.Body);
            var auth = json.ToObject<AuthenticationMessage>();
            if (response.Success) {
                if (auth.Status == true) {
                    _caligooTempData.CurrentJwtToken = new JwtSecurityToken(auth.Token);
                } else {
                    Logger.Error("CaligooLogin: authentication response error on request {0}: {1}.", url, auth.Message);
                }
            } else {
                Logger.Error("CaligooLogin: authentication response error on request {0}: {1}.", url, auth.Message);
            }
        }
        private void JwtTokenRenew() {
            var url = _caligooSettings.RefreshUrl;
            var authHeader = new AuthenticationHeaderValue("Bearer", _caligooTempData.CurrentJwtToken.RawData);
            var response = CallWebApi(url, authHeader, HttpMethod.Get);
            var json = JObject.Parse(response.Body);
            var auth = json.ToObject<AuthenticationMessage>();
            if (response.Success) {
                if (auth.Status == true) {
                    _caligooTempData.CurrentJwtToken = new JwtSecurityToken(auth.Token);
                } else {
                    Logger.Error("JwtTokenRenew: token refresh error. Message: {0}", auth.Message);
                }
            } else {
                Logger.Error("JwtTokenRenew: token refresh error. Message: {0}", auth.Message);
            }
        }
        public UserMessage GetUserDetails(string caligooUserName) {
            var resource = _caligooSettings.UsersPath;
            resource = resource.TrimEnd('/') + "/" + caligooUserName;
            var response = ResultFromCaligooApiGet(resource);
            if (response.Success) {
                var jUsers = JObject.Parse(response.Body);
                var user = jUsers.ToObject<UserMessage>();
                return user;
            }
            return null;
        }
        public List<UserMessage> GetFilteredCaligooUsers(CaligooUsersFilterValue filters) {
            var result = new List<UserMessage>();
            var response = ResultFromCaligooApiGet(_caligooSettings.UsersPath, filters.GetQueryString());
            if (response.Success) {
                var jUsers = JObject.Parse(response.Body);
                var userList = jUsers.ToObject<UserListMessage>();
                foreach (var usr in userList.Data) {
                    result.Add(new UserMessage { CaligooUserId = usr.CaligooUserId, CaligooUserName = usr.CaligooUserName });
                }
            }
            return result;
        }
        public List<string> GetFilteredCaligooUsersIds(CaligooUsersFilterValue filters) {
            return GetFilteredCaligooUsers(filters).Select(x => x.CaligooUserId).ToList();
        }
        public List<LocationMessage> GetLocations() {
            var result = new List<LocationMessage>();
            LocationMessage location = null;
            var response = ResultFromCaligooApiGet(_caligooSettings.LocationsPath);
            if (response.Success) {
                var jArr = JArray.Parse(response.Body);
                foreach (var jLoc in jArr) {
                    location = jLoc.ToObject<LocationMessage>();
                    result.Add(location);
                }
            }
            return result;
        }
        private void EnsureJwtToken() {
            var now = DateTime.UtcNow;
            if(_caligooTempData.CurrentJwtToken == null) {
                CaligooLogin();
            } else if ((_caligooTempData.CurrentJwtToken.ValidTo - now) <= new TimeSpan(0, 10, 0)) { // if there are less than 10 minutes left // TODO: verificare se renderlo parametrico
                JwtTokenRenew();
            } else if(now > _caligooTempData.CurrentJwtToken.ValidTo) {
                CaligooLogin();
            }
        }
        private CallResult ResultFromCaligooApiGet(string resource, string parameters = null) {
            var result = CallResult.Failure;
            EnsureJwtToken();
            var url = ComposeUrl(resource, parameters);
            if(_caligooTempData.CurrentJwtToken != null) {
                var authHeader = new AuthenticationHeaderValue("Bearer", _caligooTempData.CurrentJwtToken.RawData);
                result = CallWebApi(url, authHeader, HttpMethod.Get);
            }
            return result;
        }
        private CallResult ResultFromCaligooApiPost(string resource, string content, string parameters = null) {
            var result = CallResult.Failure;
            EnsureJwtToken();
            var url = ComposeUrl(resource, parameters);
            if (_caligooTempData.CurrentJwtToken != null) {
                var authHeader = new AuthenticationHeaderValue("Bearer", _caligooTempData.CurrentJwtToken.RawData);
                result = CallWebApi(url, authHeader, HttpMethod.Post, content);
            }
            return result;
        }
        private string ComposeUrl(string resource, string parameters = null) {
            var url = string.Format("{0}/{1}", _caligooSettings.BaseUrl.TrimEnd('/'), resource);
            if (parameters != null) {
                url += string.Format("?{0}", parameters);
            }
            return url;
        }
        private CallResult CallWebApi(string url, AuthenticationHeaderValue auth, HttpMethod method, string content = null) {
            var result = CallResult.Failure;
            try {
                _caligooTempData.WebApiClient.DefaultRequestHeaders.Clear();
                if (auth != null) {
                    _caligooTempData.WebApiClient.DefaultRequestHeaders.Authorization = auth;
                }
                // specify to use TLS 1.2 as default connection if needed
                if (url.ToLowerInvariant().StartsWith("https:")) {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                }
                // call web api
                Task<HttpResponseMessage> t = null;
                if (method == HttpMethod.Get) {
                    t = _caligooTempData.WebApiClient.GetAsync(url);
                } else if (method == HttpMethod.Post) {
                    t = _caligooTempData.WebApiClient.PostAsync(url, new StringContent(content));
                }
                if (t != null) {
                    t.Wait(_caligooSettings.RequestTimeoutMillis);
                    if (t.Status == System.Threading.Tasks.TaskStatus.RanToCompletion) {
                        var aux = t.Result.Content.ReadAsStringAsync();
                        aux.Wait();
                        result.Body = aux.Result;
                        if (t.Result.IsSuccessStatusCode) {
                            result.Success = true;
                        } else {
                            Logger.Error("CallWebApi: Error {1} - {2} on request {0}.", url, (int)(t.Result.StatusCode), t.Result.ReasonPhrase);
                        }
                    } else {
                        Logger.Error("CallWebApi: Timeout on request {0}.", url);
                    }
                }
            } catch (Exception ex) {
                Logger.Error(ex, "CallWebApi error.");
            }
            return result;
        }
        public void UpdateLocation(LocationMessage message) {
            if(message == null) {
                return;
            }
            var query = _orchardServices.ContentManager.Query().Where<CaligooLocationPartRecord>(x => x.CaligooLocationId == message.CaligooLocationId);
            var location = query.List<CaligooLocationPart>().FirstOrDefault();
            if(location != null) {
                location.Address = message.Address;
                location.City = message.City;
                location.Country = message.Country;
                location.DisplayName = message.DisplayName;
                location.PostalCode = message.PostalCode;
                if (message.GeographicLocation != null) {
                    location.Latitude = message.GeographicLocation.Latitude;
                    location.Longitude = message.GeographicLocation.Longitude;
                }
            } else {
                Logger.Error("UpdateLocation: CaligooLocationId '{0}' not found. Maybe it is a new location and you need to create it in Orchard with the proper Content Type.", message.CaligooLocationId);
            }
        }
        private class CallResult {
            public bool Success { get; set; }
            public string Body { get; set; }
            public static CallResult Failure {
                get {
                    return new CallResult() { Success = false, Body = "" };
                }
            }
        }
    }
}