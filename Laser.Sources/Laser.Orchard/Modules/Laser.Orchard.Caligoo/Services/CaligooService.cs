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
        ContentItem GetContactId(string caligooUserId);
        ContentItem CreateContact(string caligooUserId, string name, string surname, string email, string phone);
    }
    public class CaligooService : ICaligooService {
        private readonly IOrchardServices _orchardServices;
        private readonly ICommunicationService _communicationService;
        public CaligooService(IOrchardServices orchardServices, ICommunicationService communicationService, IUserService userService) {
            _orchardServices = orchardServices;
            _communicationService = communicationService;
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
    }
}