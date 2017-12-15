using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using System.Linq;
using Laser.Orchard.CommunicationGateway.Services;
using Orchard.Core.Common.Models;
using Orchard.Users.Services;
using Orchard.Users.Models;
using Orchard.Security;
using System.IdentityModel.Tokens;
using Laser.Orchard.Caligoo.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System;
using System.Net.Http;
using Orchard.Localization;
using Orchard.Logging;

namespace Laser.Orchard.Caligoo.Services {
    public interface ICaligooService : IDependency {
        ContentItem GetContact(string caligooUserId);
        ContentItem CreateContact(LoginLogoutEventMessage caligooUserEvent);
        void CaligooLogin(string usr, string pwd);
        void JwtTokenRenew();
        JObject GetUserDetails(string caligooUserId);
        List<string> GetFilteredCaligooUsers(CaligooUsersFilterValue filters);
        JArray GetLocations();
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
            contact.As<TitlePart>().Title = caligooUserEvent.CaligooUserId;
            contact.As<CaligooUserPart>().CaligooUserId = caligooUserEvent.CaligooUserId;
            var commonPart = contact.As<CommonPart>();
            if(commonPart != null) {
                commonPart.Owner = GetAdministrator();
            }
            _orchardServices.ContentManager.Create(contact);
            //if (string.IsNullOrWhiteSpace(email) == false) {
            //    _communicationService.AddEmailToContact(email, contact);
            //}
            //if (string.IsNullOrWhiteSpace(phone) == false) {
            //    _communicationService.AddSmsToContact("", phone, contact, false);
            //}
            return contact;
        }
        private IUser GetAdministrator() {
            IUser result = null;
            var superUser = _orchardServices.WorkContext.CurrentSite.SuperUser;
            var query = _orchardServices.ContentManager.Query().ForType("User").Where<UserPartRecord>(x => x.UserName == superUser);
            var user = query.List().FirstOrDefault();
            if (user != null) {
                result = user.As<UserPart>();
            }
            return result;
        }
        public void CaligooLogin(string usr, string pwd) {
            // TODO
            _caligooTempData.CurrentJwtToken = new JwtSecurityToken("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiYWRtaW4iOnRydWV9.TJVA95OrM7E2cBab30RMHrHDcEfxjoYZgeFONFh7HgQ"); // token valido di esempio
            _caligooTempData.Test = "prova";
        }
        public void JwtTokenRenew() {
            // TODO
        }
        public JObject GetUserDetails(string caligooUserId) {
            // TODO
            return new JObject();
        }
        public List<string> GetFilteredCaligooUsers(CaligooUsersFilterValue filters) {
            var result = new List<string>();
            var json = ResultFromCaligooApiGet("users", filters.GetQueryString());
            var jUsers = JObject.Parse(json);
            var userList = jUsers.ToObject<UserListMessage>();
            foreach(var usr in userList.Data) {
                result.Add(usr.CaligooUserId);
            }
            return result;
        }
        public JArray GetLocations() {
            // TODO
            var json = ResultFromCaligooApiGet("locations");
            return JArray.Parse(json);
        }
        private void EnsureJwtToken() {
            var now = DateTime.UtcNow;
            if(_caligooTempData.CurrentJwtToken == null) {
                CaligooLogin("", "");
            } else if ((_caligooTempData.CurrentJwtToken.ValidTo - now) <= new TimeSpan(0, 10, 0)) { // if there are less than 10 minutes left
                JwtTokenRenew();
            } else if(now > _caligooTempData.CurrentJwtToken.ValidTo) {
                CaligooLogin("", "");
            }
        }
        private string ResultFromCaligooApiGet(string resource, string parameters = null) {
            string result = null;
            EnsureJwtToken();
            var url = ComposeUrl(resource, parameters);
            _caligooTempData.WebApiClient.DefaultRequestHeaders.Clear();
            _caligooTempData.WebApiClient.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", _caligooTempData.CurrentJwtToken.RawData));
            var t = _caligooTempData.WebApiClient.GetAsync(url);
            var timeout = _caligooSettings.RequestTimeoutMillis == 0 ? 10000 : _caligooSettings.RequestTimeoutMillis; // 10000 = default timeout
            t.Wait(timeout);
            if (t.Status == System.Threading.Tasks.TaskStatus.RanToCompletion) {
                if (t.Result.IsSuccessStatusCode) {
                    var aux = t.Result.Content.ReadAsStringAsync();
                    aux.Wait();
                    result = aux.Result;
                } else {
                    Logger.Error("ResultFromCaligooApiGet: Error {1} - {2} on request {0}.", url, (int)(t.Result.StatusCode), t.Result.ReasonPhrase);
                }
            } else {
                Logger.Error("ResultFromCaligooApiGet: Timeout on request {0}.", url);
            }
            return result;
        }
        private string ResultFromCaligooApiPost(string resource, string content, string parameters = null) {
            string result = null;
            EnsureJwtToken();
            var url = ComposeUrl(resource, parameters);
            _caligooTempData.WebApiClient.DefaultRequestHeaders.Clear();
            _caligooTempData.WebApiClient.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", _caligooTempData.CurrentJwtToken.ToString()));
            var t = _caligooTempData.WebApiClient.PostAsync(url, new StringContent(content));
            var timeout = _caligooSettings.RequestTimeoutMillis == 0 ? 10000 : _caligooSettings.RequestTimeoutMillis; // 10000 = default timeout
            t.Wait(timeout);
            if (t.Status == System.Threading.Tasks.TaskStatus.RanToCompletion) {
                if (t.Result.IsSuccessStatusCode) {
                    var aux = t.Result.Content.ReadAsStringAsync();
                    aux.Wait();
                    result = aux.Result;
                } else {
                    Logger.Error("ResultFromCaligooApiPost: Error {1} - {2} on request {0}.", url, (int)(t.Result.StatusCode), t.Result.ReasonPhrase);
                }
            } else {
                Logger.Error("ResultFromCaligooApiPost: Timeout ({1:#.##0} millis) on request {0}.", url, timeout);
            }
            return result;
        }
        private string ComposeUrl(string resource, string parameters = null) {
            var url = string.Format("http://localhost/Laser.Orchard/Modules/Orchard.Blogs/Styles/menu.blog-admin.css", resource);
            if (parameters != null) {
                url += string.Format("?{0}", parameters);
            }
            return url;
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
    }
}