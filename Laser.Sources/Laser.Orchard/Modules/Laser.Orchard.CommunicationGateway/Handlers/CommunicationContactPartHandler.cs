using Laser.Orchard.CommunicationGateway.Models;
using Laser.Orchard.CommunicationGateway.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Users.Models;

namespace Laser.Orchard.CommunicationGateway.Handlers {

    public class CommunicationContactPartHandler : ContentHandler {
        public Localizer T { get; set; }
        private readonly ICommunicationService _communicationService;

        public CommunicationContactPartHandler(IRepository<CommunicationContactPartRecord> repository, ICommunicationService communicationService) {
            _communicationService = communicationService;
            Filters.Add(StorageFilter.For(repository));
            T = NullLocalizer.Instance;
            #region sync user profile
            OnUpdated<UserPart>((context, part) => UpdateProfile(context.ContentItem));
            OnRemoved<CommunicationContactPart>((context, part) => RemoveLinks(part));
            #endregion
        }

        private void UpdateProfile(ContentItem item) {
            if (item.ContentType == "User") {
                _communicationService.UserProfileToContact((IUser)item.As<IUser>());
            }
        }
        private void RemoveLinks(CommunicationContactPart item) {
            item.UserIdentifier=0;
        }
    }
}