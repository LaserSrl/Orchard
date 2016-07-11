using System.Collections.Generic;
using System.Linq;
using Laser.Orchard.Mobile.Models;
using Orchard.Data;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Users.Models;
using Orchard;
using Laser.Orchard.CommunicationGateway.Models;
using Orchard.Core.Title.Models;
using Laser.Orchard.CommunicationGateway.Services;
using Laser.Orchard.Mobile.Services;

namespace Laser.Orchard.Mobile.Handlers {

    public class MobileContactPartHandler : ContentHandler {
        private readonly IRepository<PushNotificationRecord> _deviceRepository;
        private readonly IRepository<UserDeviceRecord> _userDeviceRepository;
        private readonly IOrchardServices _orchardServices;
        private readonly ICommunicationService _communicationService;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly IRepository<SentRecord> _sentRepository;

        public MobileContactPartHandler(IRepository<MobileContactPartRecord> repository, IRepository<SentRecord> sentRepository, IRepository<PushNotificationRecord> ProviderRepository, IRepository<UserDeviceRecord> userDeviceRepository, IOrchardServices orchardServices, ICommunicationService communicationService, IPushNotificationService pushNotificationService) {
            Filters.Add(StorageFilter.For(repository));
            _deviceRepository = ProviderRepository;
            _userDeviceRepository = userDeviceRepository;
            _orchardServices = orchardServices;
            _sentRepository = sentRepository;
            _communicationService = communicationService;
            _pushNotificationService = pushNotificationService;
            Filters.Add(new ActivatingFilter<MobileContactPart>("CommunicationContact"));
            OnLoaded<MobileContactPart>(LazyLoadHandlers);
            OnRemoved<UserPart>((context, part) => { _pushNotificationService.DeleteUserDeviceAssociation(part.Id); });
            OnRemoved<CommunicationContactPart>((context, part) => { _pushNotificationService.RebindDevicesToMasterContact(part.Id); });
        }

        protected void LazyLoadHandlers(LoadContentContext context, MobileContactPart part) {
            // Add handlers that will load content for id's just-in-time
            part.MobileEntries.Loader(x => OnLoader(context));
        }

        private IList<PushNotificationRecord> OnLoader(LoadContentContext context) {
            return _deviceRepository
                    .Fetch(x => x.MobileContactPartRecord_Id == context.ContentItem.Id)
                    .Select(x => new PushNotificationRecord {
                        DataInserimento = x.DataInserimento,
                        Device = x.Device,
                        DataModifica = x.DataModifica,
                        Language = x.Language,
                        Id = x.Id,
                        Produzione = x.Produzione,
                        Token = x.Token,
                        MobileContactPartRecord_Id = x.MobileContactPartRecord_Id,
                        UUIdentifier = x.UUIdentifier,
                        Validated = x.Validated
                    })
                    .ToList();
        }
    }
}