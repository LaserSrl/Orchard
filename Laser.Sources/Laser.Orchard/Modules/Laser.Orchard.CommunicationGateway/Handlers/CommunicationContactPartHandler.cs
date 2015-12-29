using Laser.Orchard.CommunicationGateway.Models;
using Laser.Orchard.CommunicationGateway.Services;
using Laser.Orchard.StartupConfig.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Users.Models;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.CommunicationGateway.Handlers {

    public class CommunicationContactPartHandler : ContentHandler {
        public Localizer T { get; set; }
        private readonly ICommunicationService _communicationService;
        private readonly IRepository<CommunicationEmailRecord> _Emailrepository;

        public CommunicationContactPartHandler(IRepository<CommunicationEmailRecord> Emailrepository,IRepository<CommunicationContactPartRecord> repository, ICommunicationService communicationService) {
            _communicationService = communicationService;
            Filters.Add(StorageFilter.For(repository));
            _Emailrepository = Emailrepository;
            T = NullLocalizer.Instance;

            Filters.Add(new ActivatingFilter<EmailContactPart>("CommunicationContact"));
            OnLoaded<EmailContactPart>(LazyLoadEmailHandlers);

            Filters.Add(new ActivatingFilter<FavoriteCulturePart>("CommunicationContact"));

            #region sync user profile
            OnUpdated<UserPart>((context, part) => UpdateProfile(context.ContentItem));
            OnRemoved<CommunicationContactPart>((context, part) => RemoveLinks(part));
            #endregion
        }


        protected void LazyLoadEmailHandlers(LoadContentContext context, EmailContactPart part) {
            // Add handlers that will load content for id's just-in-time
            part.EmailEntries.Loader(x => OnEmailLoader(context));
        }

        private IList<CommunicationEmailRecord> OnEmailLoader(LoadContentContext context) {
            return _Emailrepository
                    .Fetch(x => x.CommunicationContactPartRecord_Id == context.ContentItem.Id)
                    .Select(x => new CommunicationEmailRecord {
                        DataInserimento = x.DataInserimento,
                        DataModifica = x.DataModifica,
                        Language = x.Language,
                        Id = x.Id,
                        Produzione = x.Produzione,
                        Email = x.Email,
                        CommunicationContactPartRecord_Id = x.CommunicationContactPartRecord_Id,
                        Validated = x.Validated
                    })
                    .ToList();
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