using Laser.Orchard.CommunicationGateway.Models;
using Laser.Orchard.CommunicationGateway.Services;
using Laser.Orchard.Mobile.Models;
using Laser.Orchard.Mobile.Settings;
using Laser.Orchard.Mobile.ViewModels;
using Laser.Orchard.Queries.Models;
using Laser.Orchard.Queries.Services;
using Newtonsoft.Json;
using NHibernate.Transform;
using Orchard;
using Orchard.Autoroute.Models;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.Data;
using Orchard.Environment.Configuration;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Tokens;
using Orchard.UI.Notify;
using Orchard.Users.Models;
using PushSharp;
using PushSharp.Google;
using PushSharp.Apple;
using PushSharp.Core;
using PushSharp.Windows;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Xml.Linq;

namespace Laser.Orchard.Mobile.Services {

    public interface IPushNotificationService : IDependency {
        void StorePushNotification(PushNotificationRecord pushElement);
        void UpdateDevice(string uuIdentifier);
        void DeleteUserDeviceAssociation(int userId);
        void RebindDevicesToMasterContact(int contactId);
        void Synchronize();
        IEnumerable<PushNotificationRecord> SearchPushNotification(string texttosearch);
    }

    public class PushNotificationService : IPushNotificationService {
        private readonly IRepository<SentRecord> _sentRepository;
        private readonly IRepository<PushNotificationRecord> _pushNotificationRepository;
        private readonly IRepository<UserDeviceRecord> _userDeviceRecord;
        private readonly IQueryPickerService _queryPickerServices;
        public Localizer T { get; set; }
        private readonly INotifier _notifier;
        private readonly IOrchardServices _orchardServices;
        private readonly IMylogService _myLog;
        private readonly ShellSettings _shellSetting;
        private readonly ISessionLocator _sessionLocator;
        public ICommunicationService _communicationService;
        private readonly ITokenizer _tokenizer;
        private readonly ITransactionManager _transactionManager;
        private Int32 messageSent;
        private object lockMonitor;
        private const int MAX_PUSH_TEXT_LENGTH = 160;

        public PushNotificationService(
            IRepository<SentRecord> sentRepository,
                IOrchardServices orchardServices,
                IRepository<PushNotificationRecord> pushNotificationRepository,
                IRepository<UserDeviceRecord> userDeviceRecord,
                INotifier notifier,
                IMylogService myLog,
                ShellSettings shellSetting,
                ISessionLocator sessionLocator,
                ITokenizer tokenizer,
                IQueryPickerService queryPickerService,
                ITransactionManager transactionManager
         ) {
            _orchardServices = orchardServices;
            _sentRepository = sentRepository;
            T = NullLocalizer.Instance;
            _pushNotificationRepository = pushNotificationRepository;
            _notifier = notifier;
            _myLog = myLog;
            _shellSetting = shellSetting;
            _sessionLocator = sessionLocator;
            _tokenizer = tokenizer;
            _userDeviceRecord = userDeviceRecord;
            _orchardServices.WorkContext.TryResolve<ICommunicationService>(out _communicationService);
            _queryPickerServices = queryPickerService;
            _transactionManager = transactionManager;
        }

        /// <summary>
        /// Elimina l'associazione user-device relativa a utenti eliminati o inesistenti.
        /// </summary>
        private void DeleteObsoleteUserDevices() {
            ContentItem user = null;
            List<UserDeviceRecord> lUdr = _userDeviceRecord.Fetch(x => x.UserPartRecord.Id > 0).ToList();
            foreach (UserDeviceRecord up in lUdr) {
                user = _orchardServices.ContentManager.Get(up.UserPartRecord.Id);
                if (user == null) {
                    _userDeviceRecord.Delete(up);
                    _userDeviceRecord.Flush();
                }
            }
        }

        public void Synchronize() {
            CommunicationContactPart master = _communicationService.EnsureMasterContact();
            _transactionManager.RequireNew();

            // assegna un contact a ogni device
            int idmaster = master.Id;
            var notificationrecords = _pushNotificationRepository.Fetch(x => x.Produzione && x.Validated).ToList();
            foreach (PushNotificationRecord rec in notificationrecords) {
                rec.MobileContactPartRecord_Id = EnsureContactId(rec.UUIdentifier, idmaster);
                _pushNotificationRepository.Update(rec);
                _transactionManager.RequireNew();
            }
            _pushNotificationRepository.Flush();
            _notifier.Add(NotifyType.Information, T("Linked {0} device To Master contact", notificationrecords.Count().ToString()));
            _myLog.WriteLog(string.Format("Linked {0} device To Master contact", notificationrecords.Count().ToString()));
            _transactionManager.RequireNew();

            // elimina gli userDevice riferiti a utenti inesistenti (perché cancellati)
            UserPart user = null;
            List<UserDeviceRecord> elencoUdr = _userDeviceRecord.Fetch(x => x.UserPartRecord.Id > 0).ToList();
            foreach (UserDeviceRecord udr in elencoUdr) {
                user = _orchardServices.ContentManager.Get<UserPart>(udr.UserPartRecord.Id);
                if (user == null) {
                    _userDeviceRecord.Delete(udr);
                    _transactionManager.RequireNew();
                }
            }
            _userDeviceRecord.Flush();
            _transactionManager.RequireNew();

            // elimina gli userDevice duplicati (con lo stesso UUIdentifier) e tiene il più recente (in base all'Id del record)
            string uuidPrecedente = "";
            elencoUdr = _userDeviceRecord.Fetch(x => x.UUIdentifier != null).OrderBy(y => y.UUIdentifier).OrderByDescending(z => z.Id).ToList();
            foreach (UserDeviceRecord udr in elencoUdr) {
                if (udr.UUIdentifier == uuidPrecedente) {
                    _userDeviceRecord.Delete(udr);
                    _transactionManager.RequireNew();
                }
                else {
                    uuidPrecedente = udr.UUIdentifier;
                }
            }
            _userDeviceRecord.Flush();
            _transactionManager.RequireNew();
        }

        #region [CRUD PushNotification]

        public void StorePushNotification(PushNotificationRecord pushElement) {
            PushNotificationRecord OldPush = _pushNotificationRepository.Fetch(x => (x.UUIdentifier == pushElement.UUIdentifier || x.Token == pushElement.Token) && x.Produzione == pushElement.Produzione && x.Device == pushElement.Device).FirstOrDefault();
            DateTime adesso = DateTime.Now;
            if (OldPush != null) { // se dispositivo già registrato sovrascrivo lo stesso record
                pushElement.DataModifica = adesso;
                pushElement.DataInserimento = OldPush.DataInserimento;
                pushElement.Id = OldPush.Id;
                pushElement.MobileContactPartRecord_Id = EnsureContactId(pushElement.UUIdentifier);
                _pushNotificationRepository.Update(pushElement);
            }
            else {
                pushElement.Id = 0;
                pushElement.DataInserimento = adesso;
                pushElement.DataModifica = adesso;
                pushElement.MobileContactPartRecord_Id = EnsureContactId(pushElement.UUIdentifier);
                
                // se è un nuovo dispositivo, registra anche host, prefix e machineName dell'ambiente corrente
                pushElement.RegistrationUrlHost = _shellSetting.RequestUrlHost ?? "";
                pushElement.RegistrationUrlPrefix = _shellSetting.RequestUrlPrefix ?? "";
                pushElement.RegistrationMachineName = System.Environment.MachineName ?? "";

                _pushNotificationRepository.Create(pushElement);
            }

            // cerca eventuali record corrispondenti in UserDevice e fa sì che ce ne sia uno solo relativo al nuovo UUIdentifier (quello con l'Id più recente)
            // eliminando eventualmente i duplicati e i record riferiti al vecchio UUIdentifier;
            UserDeviceRecord my_disp = null;
            var elencoNuovi = _userDeviceRecord.Fetch(x => x.UUIdentifier == pushElement.UUIdentifier).OrderByDescending(y => y.Id).ToList();
            foreach (var record in elencoNuovi) {
                if (my_disp == null) {
                    my_disp = record;
                }
                else {
                    _userDeviceRecord.Delete(record);
                }
            }
            if (OldPush != null && OldPush.UUIdentifier != pushElement.UUIdentifier) {
                var elencoVecchi = _userDeviceRecord.Fetch(x => x.UUIdentifier == OldPush.UUIdentifier).OrderByDescending(y => y.Id).ToList();
                foreach (var record in elencoVecchi) {
                    if (my_disp == null) {
                        // aggiorna uno dei record che aveva il vecchio UUIdentifier, quello con l'Id più recente
                        my_disp = record;
                        my_disp.UUIdentifier = pushElement.UUIdentifier;
                        _userDeviceRecord.Update(my_disp);
                    }
                    else {
                        _userDeviceRecord.Delete(record);
                    }
                }
            }
        }

        /// <summary>
        /// Restituisce l'Id del contact relativo allo UUIdentifier specificato.
        /// Se non trova un contact corrispondente, restituisce l'Id del Master Contact.
        /// </summary>
        /// <param name="uuIdentifier"></param>
        /// <returns></returns>
        private int EnsureContactId(string uuIdentifier) {
            int contactId = 0;
            try {
                var userDevice = _userDeviceRecord.Fetch(x => x.UUIdentifier == uuIdentifier).FirstOrDefault();
                if (userDevice != null) {
                    var contact = _communicationService.TryEnsureContact(userDevice.UserPartRecord.Id);
                    if (contact != null) {
                        contactId = contact.Id;
                    }
                }
                // se non trova un contact a cui agganciarlo, lo aggancia al Master Contact
                if (contactId == 0) {
                    var masterContact = _communicationService.EnsureMasterContact();
                    contactId = masterContact.Id;
                }
            }
            catch (Exception ex) {
                _myLog.WriteLog(string.Format("EnsureContactId - Exception occurred: {0} \r\n    in {1}", ex.Message, ex.StackTrace));
            }
            return contactId;
        }

        /// <summary>
        /// Metodo ottimizzato per l'elaborazione di molti record (ad esempio nella Synchronize).
        /// </summary>
        /// <param name="uuIdentifier"></param>
        /// <param name="masterContactId"></param>
        /// <returns></returns>
        private int EnsureContactId(string uuIdentifier, int masterContactId) {
            int contactId = 0;
            try {
                var userDevice = _userDeviceRecord.Fetch(x => x.UUIdentifier == uuIdentifier).FirstOrDefault();
                if (userDevice != null) {
                    var contact = _communicationService.TryEnsureContact(userDevice.UserPartRecord.Id);
                    if (contact != null) {
                        contactId = contact.Id;
                    }
                }
                // se non trova un contact a cui agganciarlo, lo aggancia al Master Contact
                if (contactId == 0) {
                    contactId = masterContactId;
                }
            }
            catch (Exception ex) {
                _myLog.WriteLog(string.Format("EnsureContactId(string, int) - Exception occurred: {0} \r\n    in {1}", ex.Message, ex.StackTrace));
            }
            return contactId;
        }

        /// <summary>
        /// Aggiorna il legame tra device e contact se il device è registrato.
        /// </summary>
        /// <param name="uuIdentifier"></param>
        public void UpdateDevice(string uuIdentifier) {
            var device = _pushNotificationRepository.Fetch(x => x.UUIdentifier == uuIdentifier).FirstOrDefault();
            if (device != null) {
                StorePushNotification(device);
            }
        }

        public void DeleteUserDeviceAssociation(int userId) {
            var userDevices = _userDeviceRecord.Fetch(x => x.UserPartRecord.Id == userId);
            foreach (var userDevice in userDevices) {
                _userDeviceRecord.Delete(userDevice);
            }
            _userDeviceRecord.Flush();
        }

        public void RebindDevicesToMasterContact(int contactId) {
            var masterContact = _communicationService.EnsureMasterContact();
            var elencoDevice = _pushNotificationRepository.Fetch(x => x.MobileContactPartRecord_Id == contactId).ToList();
            foreach (var device in elencoDevice) {
                device.MobileContactPartRecord_Id = masterContact.Id;
                _pushNotificationRepository.Update(device);
            }
            _pushNotificationRepository.Flush();
        }

        private PushNotificationRecord GetPushNotificationBy_UUIdentifier(string uuidentifier, bool produzione) {
            return _pushNotificationRepository.Fetch(x => x.UUIdentifier == uuidentifier && x.Produzione == produzione).FirstOrDefault();
        }

        public IEnumerable<PushNotificationRecord> SearchPushNotification(string texttosearch) {
            return _pushNotificationRepository.Fetch(x => x.UUIdentifier.Contains(texttosearch)).ToList();
        }

        #endregion [CRUD PushNotification]
    }
}