using System.Collections.Generic;
using System.Linq;

using Laser.Orchard.Mobile.Models;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;

namespace Laser.Orchard.Mobile.Handlers {

    public class MobileContactPartHandler : ContentHandler {
        private readonly IRepository<PushNotificationRecord> _ProviderRepository;

        public MobileContactPartHandler(IRepository<PushNotificationRecord> ProviderRepository) {
            _ProviderRepository = ProviderRepository;
            Filters.Add(new ActivatingFilter<MobileContactPart>("CommunicationContact"));
            OnLoaded<MobileContactPart>(LazyLoadHandlers);
        }

        protected void LazyLoadHandlers(LoadContentContext context, MobileContactPart part) {
            // Add handlers that will load content for id's just-in-time
            part.MobileEntries.Loader(x => OnLoader(context));
        }

        private IList<PushNotificationRecord> OnLoader(LoadContentContext context) {
            return _ProviderRepository
                    .Fetch(x => x.CommunicationContactPartRecord_Id == context.ContentItem.Id)
                    .Select(x => new PushNotificationRecord {
                        DataInserimento=x.DataInserimento,
                        Device=x.Device,
                        DataModifica=x.DataModifica,
                        Language=x.Language,
                        Id = x.Id,
                        Produzione=x.Produzione,
                        Token=x.Token,
                        CommunicationContactPartRecord_Id = x.CommunicationContactPartRecord_Id,
                        UUIdentifier=x.UUIdentifier,
                        Validated=x.Validated
                    })
                    .ToList();
        }
    }
}