using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Core.Title.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.CommunicationGateway.Models;
using Laser.Orchard.CommunicationGateway.Services;
using Orchard.Core.Common.Models;
using Orchard.Users.Services;
using Orchard.Users.Models;
using Orchard.Security;

namespace Laser.Orchard.Caligoo.Services {
    public interface ICaligooService : IDependency {
        int GetContactId(string caligooUserId);
        int CreateContact(string caligooUserId, string name, string surname, string email, string phone);
    }
    public class CaligooService : ICaligooService {
        private readonly IOrchardServices _orchardServices;
        private readonly ICommunicationService _communicationService;
        public CaligooService(IOrchardServices orchardServices, ICommunicationService communicationService, IUserService userService) {
            _orchardServices = orchardServices;
            _communicationService = communicationService;
        }
        public int GetContactId(string caligooUserId) {
            var result = 0;
            var query =_orchardServices.ContentManager.Query().ForType("CommunicationContact").Where<TitlePartRecord>(x => x.Title == caligooUserId);
            var contacts = query.List();
            if(contacts.Any()) {
                result = contacts.First().Id;
            }
            return result;
        }
        public int CreateContact(string caligooUserId, string name, string surname, string email, string phone) {
            var contactId = 0;
            var contact = _orchardServices.ContentManager.New("CommunicationContact");
            contact.As<TitlePart>().Title = caligooUserId;
            var commonPart = contact.As<CommonPart>();
            if(commonPart != null) {
                commonPart.Owner = GetAdministrator();
            }
            _orchardServices.ContentManager.Create(contact);
            contactId = contact.Id;
            if (string.IsNullOrWhiteSpace(email) == false) {
                _communicationService.AddEmailToContact(email, contact);
            }
            if (string.IsNullOrWhiteSpace(phone) == false) {
                _communicationService.AddSmsToContact("", phone, contact, false);
            }
            return contactId;
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
    }
}