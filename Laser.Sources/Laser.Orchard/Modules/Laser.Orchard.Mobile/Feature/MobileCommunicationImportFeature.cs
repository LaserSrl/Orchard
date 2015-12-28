using Laser.Orchard.CommunicationGateway.Models;
using Laser.Orchard.CommunicationGateway.Services;
using Laser.Orchard.Mobile.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.Data;
using Orchard.Environment;
using Orchard.Environment.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.Mobile.Feature {

    [OrchardFeature("Laser.Orchard.MobileCommunicationImport")]
    public class MobileCommunicationImportFeature : IFeatureEventHandler {
        private readonly IOrchardServices _orchardServices;
        private readonly ICommunicationService _communicationService;
        private readonly IRepository<UserDeviceRecord> _repositoryUserDeviceRecord;
        private readonly IRepository<PushNotificationRecord> _repositoryPushNotificationRecord;

        public MobileCommunicationImportFeature(IRepository<UserDeviceRecord> repositoryUserDeviceRecord, IOrchardServices orchardServices, ICommunicationService communicationService, IRepository<PushNotificationRecord> repositoryPushNotificationRecord) {
            _orchardServices = orchardServices;
            _communicationService = communicationService;
            _repositoryUserDeviceRecord = repositoryUserDeviceRecord;
            _repositoryPushNotificationRecord = repositoryPushNotificationRecord;
        }

        public void Disabled(global::Orchard.Environment.Extensions.Models.Feature feature) {
            //throw new NotImplementedException();
        }

        public void Disabling(global::Orchard.Environment.Extensions.Models.Feature feature) {
            //throw new NotImplementedException();
        }

        public void Enabled(global::Orchard.Environment.Extensions.Models.Feature feature) {
            if (feature.Descriptor.Id == "Laser.Orchard.MobileCommunicationImport") {
                List<UserDeviceRecord> lUdr = _repositoryUserDeviceRecord.Fetch(x => x.UserPartRecord.Id > 0).ToList();
                foreach (UserDeviceRecord up in lUdr) {
                    CommunicationContactPart ciCommunication = _communicationService.GetContactFromUser(up.UserPartRecord.Id);
                    if (ciCommunication == null) {
                        // Una contact part dovrebbe esserci in quanto questo codice viene eseguito dopo la sincronizzazione utenti
                        // Se non vi è una contartpart deduco che il dato sia sporco (es: UUid di un utente che è stato cancellato quindi non sincronizzo il dato con contactpart, verrà legato come se fosse scollegato al contentitem che raggruppa tutti i scollegati)
                        //throw new Exception("Utente senza associazione alla profilazione");
                    }
                    else {
                        int idci = ciCommunication.ContentItem.Id;
                        var records = _repositoryPushNotificationRecord.Fetch(x => x.UUIdentifier == up.UUIdentifier).ToList();
                        foreach (PushNotificationRecord rec in records) {
                            rec.CommunicationContactPartRecord_Id = idci;
                        }
                        _repositoryPushNotificationRecord.Flush();
                    }
                }

                #region [lego i i rimanenti content al Content Master per renderli querabili]

                if (_orchardServices.ContentManager.Query<CommunicationContactPart, CommunicationContactPartRecord>().Where(y => y.Master).Count() == 0) {
                    var Contact = _orchardServices.ContentManager.New("CommunicationContact");
                    _orchardServices.ContentManager.Create(Contact);
                    Contact.As<TitlePart>().Title = "Master Content";
                    Contact.As<CommunicationContactPart>().Master = true;
                }
                CommunicationContactPart master = _orchardServices.ContentManager.Query<CommunicationContactPart, CommunicationContactPartRecord>().Where(y => y.Master).List().FirstOrDefault();
                int idmaster = master.Id;
                var notificationrecords = _repositoryPushNotificationRecord.Fetch(x => x.CommunicationContactPartRecord_Id == 0).ToList();
                foreach (PushNotificationRecord rec in notificationrecords) {
                    rec.CommunicationContactPartRecord_Id = idmaster;
                }
                _repositoryPushNotificationRecord.Flush();

                #endregion [lego i i rimanenti content al Content Master per renderli querabili]
            }
        }

        public void Enabling(global::Orchard.Environment.Extensions.Models.Feature feature) {
            //throw new NotImplementedException();
        }

        public void Installed(global::Orchard.Environment.Extensions.Models.Feature feature) {
            //throw new NotImplementedException();
        }

        public void Installing(global::Orchard.Environment.Extensions.Models.Feature feature) {
            //throw new NotImplementedException();
        }

        public void Uninstalled(global::Orchard.Environment.Extensions.Models.Feature feature) {
            //throw new NotImplementedException();
        }

        public void Uninstalling(global::Orchard.Environment.Extensions.Models.Feature feature) {
            //throw new NotImplementedException();
        }
    }
}