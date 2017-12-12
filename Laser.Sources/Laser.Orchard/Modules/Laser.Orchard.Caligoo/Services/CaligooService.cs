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

namespace Laser.Orchard.Caligoo.Services {
    public interface ICaligooService : IDependency {
        ContentItem GetContactId(string caligooUserId);
        ContentItem CreateContact(string caligooUserId, string name, string surname, string email, string phone);
        void CaligooLogin(string usr, string pwd);
        void JwtTokenRenew();
        JObject GetUserDetails(string caligooUserId);
        List<int> GetFilteredContacts(CaligooUsersFilterValue filters);
        JObject GetLocations();
    }
    public class CaligooService : ICaligooService {
        private readonly IOrchardServices _orchardServices;
        private readonly ICommunicationService _communicationService;
        private readonly CaligooTempData _caligooTempData;
        public CaligooService(IOrchardServices orchardServices, ICommunicationService communicationService, IUserService userService, CaligooTempData caligooTempData) {
            _orchardServices = orchardServices;
            _communicationService = communicationService;
            _caligooTempData = caligooTempData;
        }
        public ContentItem GetContactId(string caligooUserId) {
            ContentItem result = null;
            var query =_orchardServices.ContentManager.Query().ForType("CommunicationContact").Where<TitlePartRecord>(x => x.Title == caligooUserId);
            var contacts = query.List();
            if(contacts.Any()) {
                result = contacts.First();
            }
            return result;
        }
        public ContentItem CreateContact(string caligooUserId, string name, string surname, string email, string phone) {
            var contact = _orchardServices.ContentManager.New("CommunicationContact");
            contact.As<TitlePart>().Title = caligooUserId;
            var commonPart = contact.As<CommonPart>();
            if(commonPart != null) {
                commonPart.Owner = GetAdministrator();
            }
            _orchardServices.ContentManager.Create(contact);
            if (string.IsNullOrWhiteSpace(email) == false) {
                _communicationService.AddEmailToContact(email, contact);
            }
            if (string.IsNullOrWhiteSpace(phone) == false) {
                _communicationService.AddSmsToContact("", phone, contact, false);
            }
            return contact;
        }
        private IUser GetAdministrator() {
            IUser result = null;
            var superUser = _orchardServices.WorkContext.CurrentSite.SuperUser;
            var query = _orchardServices.ContentManager.Query().ForType("User").Where<UserPartRecord>(x => x.UserName == superUser);
            var users = query.List();
            if (users.Any()) {
                result = users.First().As<UserPart>();
            }
            return result;
        }
        public void CaligooLogin(string usr, string pwd) {
            // TODO
            //_caligooTempData.CurrentJwtToken = new JwtSecurityToken("prova");
            _caligooTempData.Test = "prova";
        }
        public void JwtTokenRenew() {
            // TODO
        }
        public JObject GetUserDetails(string caligooUserId) {
            // TODO
            return new JObject();
        }
        public List<int> GetFilteredContacts(CaligooUsersFilterValue filters) {
            // TODO
            return new List<int>();
        }
        public JObject GetLocations() {
            // TODO
            return new JObject();
        }
    }
}