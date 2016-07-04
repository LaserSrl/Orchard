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

namespace Laser.Orchard.Mobile.Handlers {

    public class MobileContactPartHandler : ContentHandler {
        private readonly IRepository<PushNotificationRecord> _deviceRepository;
        private readonly IRepository<UserDeviceRecord> _userDeviceRepository;
        private readonly IOrchardServices _orchardServices;
        private readonly ICommunicationService _communicationService;
        private readonly IRepository<SentRecord> _sentRepository;

        public MobileContactPartHandler(IRepository<MobileContactPartRecord> repository,IRepository<SentRecord> sentRepository, IRepository<PushNotificationRecord> ProviderRepository, IRepository<UserDeviceRecord> userDeviceRepository, IOrchardServices orchardServices, ICommunicationService communicationService) {
            Filters.Add(StorageFilter.For(repository));
            _deviceRepository = ProviderRepository;
            _userDeviceRepository = userDeviceRepository;
            _orchardServices = orchardServices;
            _sentRepository = sentRepository;
            _communicationService = communicationService;
            Filters.Add(new ActivatingFilter<MobileContactPart>("CommunicationContact"));
            OnLoaded<MobileContactPart>(LazyLoadHandlers);
            OnRemoved<UserPart>(RebindDevices);
            OnUpdated<CommunicationContactPart>((context, part) => {
                if (part.ContentItem.ContentType == "CommunicationContact") {
                    // TODO
                    // allinea i device del contatto in base a quelli dell'utente collegato (part.UserIdentifier)
                }
            });
        }

        protected void LazyLoadHandlers(LoadContentContext context, MobileContactPart part) {
            // Add handlers that will load content for id's just-in-time
            part.MobileEntries.Loader(x => OnLoader(context));
        }

        private void RebindDevices(RemoveContentContext context, UserPart userPart) {
            CommunicationContactPart master = _communicationService.EnsureMasterContact();

            // associa i device dell'utente al master contact
            var userDevices = _userDeviceRepository.Fetch(x => x.UserPartRecord.Id == userPart.Id);
            foreach (var userDevice in userDevices) {
                var devices = _deviceRepository.Fetch(x => x.UUIdentifier == userDevice.UUIdentifier);
                foreach (var device in devices) {
                    device.MobileContactPartRecord_Id = master.Id;
                    _deviceRepository.Update(device);
                }
                // elimina l'associazione tra utente e device eliminando lo UserDeviceRecord
                _userDeviceRepository.Delete(userDevice);
            }
        }

        private IList<PushNotificationRecord> OnLoader(LoadContentContext context) {
            return _deviceRepository
                    .Fetch(x => x.MobileContactPartRecord_Id == context.ContentItem.Id)
                    .Select(x => new PushNotificationRecord {
                        DataInserimento=x.DataInserimento,
                        Device=x.Device,
                        DataModifica=x.DataModifica,
                        Language=x.Language,
                        Id = x.Id,
                        Produzione=x.Produzione,
                        Token=x.Token,
                        MobileContactPartRecord_Id = x.MobileContactPartRecord_Id,
                        UUIdentifier=x.UUIdentifier,
                        Validated=x.Validated
                    })
                    .ToList();
        }
    }
}