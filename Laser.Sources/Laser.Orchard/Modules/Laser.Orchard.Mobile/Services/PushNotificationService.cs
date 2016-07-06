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

        IList GetPushQueryResult(Int32[] ids, bool countOnly = false);
        IList GetPushQueryResult(Int32[] ids, TipoDispositivo? tipodisp, bool produzione, string language, bool countOnly = false);
        void StorePushNotification(PushNotificationRecord pushElement);
        void UpdateDevice(string uuIdentifier);
        void DeleteUserDeviceAssociation(int userId);
        void RebindDevicesToMasterContact(int contactId);

        IEnumerable<PushNotificationRecord> SearchPushNotification(string texttosearch);

        void PublishedPushEventTest(ContentItem ci);
        void PublishedPushEvent(ContentItem ci);
        void SendPushService(bool produzione, string device, Int32 idContentRelated, string language_param, string messageApple, string messageAndroid, string JsonAndroid, string messageWindows, string sound, string queryDevice = "");
        void Synchronize();
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
            messageSent = 0;
            _shellSetting = shellSetting;
            _sessionLocator = sessionLocator;
            _tokenizer = tokenizer;
            _userDeviceRecord = userDeviceRecord;
            _orchardServices.WorkContext.TryResolve<ICommunicationService>(out _communicationService);
            _queryPickerServices = queryPickerService;
            _transactionManager = transactionManager;
        }

        public IList GetPushQueryResult(Int32[] ids, bool countOnly = false) {
            return GetPushQueryResult(ids, null, true, "All", countOnly);
        }

        public IList GetPushQueryResult(Int32[] ids, TipoDispositivo? tipodisp, bool produzione, string language, bool countOnly = false) {
            IHqlQuery query;
            if (ids != null && ids.Count() > 0) {
                query = IntegrateAdditionalConditions(_queryPickerServices.GetCombinedContentQuery(ids, null, new string[] { "CommunicationContact" }));
            }
            else {
                query = IntegrateAdditionalConditions(null);
            }

            // Trasformo in stringa HQL
            var stringHQL = ((DefaultHqlQuery)query).ToHql(false);

            // Rimuovo la Order by per poter fare la query annidata
            // TODO: trovare un modo migliore per rimuovere la order by
            // provare a usare: query.OrderBy(null, null);
            stringHQL = stringHQL.ToString().Replace("order by civ.Id", "");

            string queryForPush = "";
            if (countOnly) {
                queryForPush = "SELECT count(MobileRecord) as Tot, sum(case MobileRecord.Device when 'Android' then 1 else 0 end) as Android, sum(case MobileRecord.Device when 'Apple' then 1 else 0 end) as Apple, sum(case MobileRecord.Device when 'WindowsMobile' then 1 else 0 end) as WindowsMobile";
            }
            else {
                queryForPush = "SELECT MobileRecord.Id as Id, MobileRecord.Device as Device, MobileRecord.Produzione as Produzione, MobileRecord.Validated as Validated, MobileRecord.Language as Language, MobileRecord.UUIdentifier as UUIdentifier, MobileRecord.Token as Token";
            }
            queryForPush += " FROM Orchard.ContentManagement.Records.ContentItemVersionRecord as civr " +
                "join civr.ContentItemRecord as cir " +
                "join cir.CommunicationContactPartRecord as CommunicationContact " +
                "join cir.MobileContactPartRecord as MobileContact " +
                "join MobileContact.MobileRecord as MobileRecord " +
                "WHERE civr.Published=1 AND MobileRecord.Validated";
            if (tipodisp.HasValue) {
                queryForPush += " AND MobileRecord.Device='" + tipodisp.Value + "'";
            }
            if (language != "All") {
                queryForPush += " AND MobileRecord.Language='" + language.Replace("'", "''") + "'"; // sostituzione anti sql-injection
            }
            queryForPush += " AND MobileRecord.Produzione=" + ((produzione) ? "1" : "0");
            if ((ids != null) && (ids.Count() > 0)) {
                // tiene conto degli id selezionati ma aggiunge comunque i device del master contact
                queryForPush += " AND (civr.Id in (" + stringHQL + ") OR CommunicationContact.Master)";
            }

            // Creo query ottimizzata per le performance
            var fullStatement = _sessionLocator.For(null)
                .CreateQuery(queryForPush)
                .SetCacheable(false);
            var lista = fullStatement
                .SetResultTransformer(Transformers.AliasToEntityMap)
                 .List();
            return lista;
        }

        private IHqlQuery IntegrateAdditionalConditions(IHqlQuery query) {
            if (query == null) {
                query = _orchardServices.ContentManager.HqlQuery().ForType(new string[] { "MobileContact" });
            }
            query = query
                .Where(x => x.ContentPartRecord<MobileContactPartRecord>(), x => x.IsNotEmpty("MobileRecord"));

            return query;
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

        #endregion [CRUD PushNotification]

        public IEnumerable<PushNotificationRecord> SearchPushNotification(string texttosearch) {
            return _pushNotificationRepository.Fetch(x => x.UUIdentifier.Contains(texttosearch)).ToList();
        }

        /// <summary>
        /// For Android
        /// idContentRelated
        /// message
        ///
        /// For Apple
        /// idContentRelated
        /// message
        /// sound
        /// </summary>
        /// <param name="produzione">send push to production or development device </param>
        /// <param name="device">All,Android,Apple</param>
        /// <param name="idContentRelated">0 :default (if content is linked no push will be sent until contend is published)</param>
        /// <param name="language_param">"" for All</param>
        /// <param name="messageApple"></param>
        /// <param name="messageAndroid">sent if JsonAndroid is empty </param>
        /// <param name="JsonAndroid">If JsonAndroid is empty messageAndroid will be sent</param>
        /// <param name="messageWindows"></param>
        /// <param name="sound">Used in Apple Message</param>
        public void SendPushService(bool produzione, string device, Int32 idContentRelated, string language_param, string messageApple, string messageAndroid, string JsonAndroid, string messageWindows, string sound, string queryDevice = "") {
            bool stopPush = false;
            ContentItem relatedContentItem = null;
            string ctype = "";
            string displayalias = "";
            if (idContentRelated > 0) {
                relatedContentItem = _orchardServices.ContentManager.Get(idContentRelated);
                if (!relatedContentItem.IsPublished()) {
                    _notifier.Information(T("No push will be sent, related content must be published"));
                    _myLog.WriteLog("No push will be sent, related content must be published");
                    stopPush = true;
                }
                var extra = getextrainfo(idContentRelated);
                ctype = extra[0];
                displayalias = extra[1];
            }
            else { idContentRelated = 0; }
            if (!stopPush) {
                string language = "";
                if (string.IsNullOrEmpty(language_param)) {
                    language = _orchardServices.WorkContext.CurrentSite.SiteCulture;
                    try {
                        language = ((dynamic)relatedContentItem).LocalizationPart.Culture != null ? ((dynamic)relatedContentItem).LocalizationPart.Culture.Culture : language;
                    }
                    catch {
                        language = "All";
                    }
                }
                else
                    language = language_param;
                _myLog.WriteLog("language:" + language);
                _myLog.WriteLog("Send to:" + device);
                if (device == "All") {
                    if (string.IsNullOrEmpty(JsonAndroid) || JsonAndroid.Trim() == "") {
                        PushAndroidVM pushandroid = new PushAndroidVM();
                        pushandroid.Id = 0;
                        pushandroid.Rid = idContentRelated;
                        pushandroid.Text = messageAndroid;
                        pushandroid.Ct = ctype;
                        pushandroid.Al = displayalias;
                        SendAllAndroid("unknown", pushandroid, produzione, language, queryDevice);
                    }
                    else {
                        SendAllAndroidJson(0, "unknown", JsonAndroid, produzione, language, queryDevice);
                    }

                    PushAppleVM pushapple = new PushAppleVM();
                    pushapple.idContent = 0;
                    pushapple.idRelated = idContentRelated;
                    pushapple.Sound = sound;
                    pushapple.Text = messageApple;
                    pushapple.Title = "";
                    pushapple.Ct = ctype;
                    pushapple.Al = displayalias;
                    pushapple.ValidPayload = true;
                    SendAllApple("unknown", pushapple, produzione, language, queryDevice);
                    //TODO: windows
                    //SendAllWindowsMobile(ci.As<MobilePushPart>(), idContent, idContentRelated, language);
                }
                if (device == TipoDispositivo.Android.ToString()) {
                    if (string.IsNullOrEmpty(JsonAndroid) || JsonAndroid.Trim() == "") {
                        PushAndroidVM pushandroid = new PushAndroidVM();
                        pushandroid.Id = 0;
                        pushandroid.Rid = idContentRelated;
                        pushandroid.Text = messageAndroid;
                        pushandroid.Ct = ctype;
                        pushandroid.Al = displayalias;
                        SendAllAndroid("unknown", pushandroid, produzione, language, queryDevice);
                    }
                    else {
                        SendAllAndroidJson(0, "unknown", JsonAndroid, produzione, language, queryDevice);
                    }
                }
                if (device == TipoDispositivo.Apple.ToString()) {
                    PushAppleVM pushapple = new PushAppleVM();
                    pushapple.idContent = 0;
                    pushapple.idRelated = idContentRelated;
                    pushapple.Sound = sound;
                    pushapple.Text = messageApple;
                    pushapple.Title = "";
                    pushapple.Ct = ctype;
                    pushapple.Al = displayalias;
                    pushapple.ValidPayload = true;
                    SendAllApple("unknown", pushapple, produzione, language, queryDevice);
                }
                //TODO: windows
                //if (device == TipoDispositivo.WindowsMobile.ToString()) {
                //    SendAllWindowsMobile(ci.As<MobilePushPart>(), idContent, idContentRelated, language);
                //}
            }
        }

        public void PublishedPushEventTest(ContentItem ci) {
            int maxIdVersionRecord = _orchardServices.ContentManager.GetAllVersions(ci.Id).Max(x => x.VersionRecord.Id);
            ContentItem savedCi = _orchardServices.ContentManager.GetAllVersions(ci.Id).Where(x => x.VersionRecord.Id == maxIdVersionRecord).FirstOrDefault();
            MobilePushPart mpp = ci.As<MobilePushPart>();
            Int32 idContent = mpp.Id;
            var relatedContent = ((dynamic)ci).MobilePushPart.RelatedContent;

            // nel caso in cui la MobilePushPart sia contenuta nel content type CommunicationAdvertising, usa il related content di quest'ultimo
            if (ci.ContentType == "CommunicationAdvertising") {
                relatedContent = ((dynamic)savedCi).CommunicationAdvertisingPart.ContentLinked;
            }

            ContentItem relatedContentItem = null;
            Int32 idContentRelated = 0;
            dynamic contentForPush;
            dynamic ciDynamic = (dynamic)ci;

            if (relatedContent != null && relatedContent.Ids != null && ((int[])relatedContent.Ids).Count() > 0) {
                idContentRelated = relatedContent.Ids[0];
                relatedContentItem = _orchardServices.ContentManager.Get(idContentRelated);
                if (relatedContentItem == null) {
                    relatedContentItem = _orchardServices.ContentManager.GetLatest(idContentRelated);
                }
                contentForPush = (dynamic)relatedContentItem;
            }
            else {
                contentForPush = ciDynamic;
            }

            // tipo dispositivo (Android, Apple, Windows)
            TipoDispositivo? locTipoDispositivo = null;
            if (mpp.DevicePush != "All") {
                TipoDispositivo auxTipoDispositivo;
                if (Enum.TryParse<TipoDispositivo>(mpp.DevicePush, out auxTipoDispositivo)) {
                    locTipoDispositivo = auxTipoDispositivo;
                }
            }

            // determina il language
            string language = _orchardServices.WorkContext.CurrentSite.SiteCulture;
            try {
                language = contentForPush.LocalizationPart.Culture != null ? contentForPush.LocalizationPart.Culture.Culture : language;
            }
            catch (Exception ex) {
                language = "All";
            }

            // determina le query impostate
            int[] ids = null;
            var aux = ci.As<QueryPickerPart>();
            if (aux != null) {
                ids = aux.Ids;
            }

            bool produzione = false;

            var Myobject = new Dictionary<string, object> { { "Content", ci } };
            string queryDevice = GetQueryDevice(Myobject, ci.As<MobilePushPart>());

            if (locTipoDispositivo.HasValue == false) // tutti
                    {
                SendAllAndroidPart(mpp, idContent, idContentRelated, language, produzione, queryDevice, ids);

                SendAllApplePart(mpp, idContent, idContentRelated, language, produzione, queryDevice, ids);

                SendAllWindowsMobilePart(mpp, idContent, idContentRelated, language, produzione, queryDevice, ids);

            }
            else if (locTipoDispositivo.Value == TipoDispositivo.Android) {
                SendAllAndroidPart(mpp, idContent, idContentRelated, language, produzione, queryDevice, ids);
            }
            else if (locTipoDispositivo.Value == TipoDispositivo.Apple) {
                SendAllApplePart(mpp, idContent, idContentRelated, language, produzione, queryDevice, ids);
            }
            else if (locTipoDispositivo.Value == TipoDispositivo.WindowsMobile) {
                SendAllWindowsMobilePart(mpp, idContent, idContentRelated, language, produzione, queryDevice, ids);
            }
        }

        public void PublishedPushEvent(ContentItem ci) {
            try {
                _myLog.WriteLog("Iniziato invio Push del content " + ci.Id);
                ContentItem savedCi = _orchardServices.ContentManager.Get(ci.Id);
                MobilePushPart mpp = ci.As<MobilePushPart>();
                if ((mpp.ToPush) && (mpp.PushSent == false)) {
                    bool stopPush = false;
                    Int32 idContent = mpp.Id;
                    var relatedContent = ((dynamic)ci).MobilePushPart.RelatedContent;

                    // nel caso in cui la MobilePushPart sia contenuta nel content type CommunicationAdvertising, usa il related content di quest'ultimo
                    if (ci.ContentType == "CommunicationAdvertising") {
                        relatedContent = ((dynamic)savedCi).CommunicationAdvertisingPart.ContentLinked;
                    }
                    ContentItem relatedContentItem = null;
                    Int32 idContentRelated = 0;
                    dynamic contentForPush;
                    dynamic ciDynamic = (dynamic)ci;
                    if (relatedContent != null && relatedContent.Ids != null && ((int[])relatedContent.Ids).Count() > 0) {
                        idContentRelated = relatedContent.Ids[0];
                        relatedContentItem = _orchardServices.ContentManager.Get(idContentRelated);
                        if (relatedContentItem == null) {
                            relatedContentItem = _orchardServices.ContentManager.GetLatest(idContentRelated);
                        }
                        contentForPush = (dynamic)relatedContentItem;
                        if (!relatedContentItem.IsPublished()) {
                            _notifier.Information(T("No push will be sent, related content must be published"));
                            _myLog.WriteLog("No push will be sent, related content must be published");
                            stopPush = true;
                        }
                    }
                    else {
                        contentForPush = ciDynamic;
                    }
                    if (!stopPush) {
                        // determina le query impostate
                        int[] ids = null;
                        var aux = ci.As<QueryPickerPart>();
                        if (aux != null) {
                            ids = aux.Ids;
                        }

                        // determina il language
                        string language = _orchardServices.WorkContext.CurrentSite.SiteCulture;
                        try {
                            language = contentForPush.LocalizationPart.Culture != null ? contentForPush.LocalizationPart.Culture.Culture : language;
                        }
                        catch {
                            language = "All";
                        }
                        _myLog.WriteLog("language:" + language);
                        _myLog.WriteLog("Send to:" + mpp.DevicePush);

                        // determina se è ambiente di produzione
                        bool produzione = true;
                        if (_orchardServices.WorkContext.CurrentSite.As<PushMobileSettingsPart>().ShowTestOptions) {
                            produzione = !(mpp.TestPush);
                        }

                        // tipo didispositivo (Android, Apple, Windows)
                        TipoDispositivo? locTipoDispositivo = null;
                        if (mpp.DevicePush != "All") {
                            TipoDispositivo auxTipoDispositivo;
                            if (Enum.TryParse<TipoDispositivo>(mpp.DevicePush, out auxTipoDispositivo)) {
                                locTipoDispositivo = auxTipoDispositivo;
                            }
                        }

                        var Myobject = new Dictionary<string, object> { { "Content", ci } };
                        string queryDevice = GetQueryDevice(Myobject, ci.As<MobilePushPart>());

                        if (locTipoDispositivo.HasValue == false) // tutti
                        {
                            SendAllAndroidPart(mpp, idContent, idContentRelated, language, produzione, queryDevice, ids);

                            SendAllApplePart(mpp, idContent, idContentRelated, language, produzione, queryDevice, ids);

                            SendAllWindowsMobilePart(mpp, idContent, idContentRelated, language, produzione, queryDevice, ids);
                        }
                        else if (locTipoDispositivo.Value == TipoDispositivo.Android) {
                            SendAllAndroidPart(mpp, idContent, idContentRelated, language, produzione, queryDevice, ids);
                        }
                        else if (locTipoDispositivo.Value == TipoDispositivo.Apple) {
                            SendAllApplePart(mpp, idContent, idContentRelated, language, produzione, queryDevice, ids);
                        }
                        else if (locTipoDispositivo.Value == TipoDispositivo.WindowsMobile) {
                            SendAllWindowsMobilePart(mpp, idContent, idContentRelated, language, produzione, queryDevice, ids);
                        }
                        // aggiorna la MobilePushPart
                        mpp.PushSent = true;
                        mpp.PushSentNumber = messageSent;
                        int counter = 0;
                        if (ci.ContentType == "CommunicationAdvertising") {
                            var counterAux = GetPushQueryResult(ids, locTipoDispositivo, produzione, language, true);
                            counter = Convert.ToInt32(((Hashtable)(counterAux[0]))["Tot"]);
                        }
                        else {
                            if (queryDevice.Trim() == "") {
                                counter = _pushNotificationRepository.Fetch(x => (x.Device == locTipoDispositivo || locTipoDispositivo == null) && x.Produzione == produzione && x.Validated == true && (x.Language == language || language == "All")).Count();
                            }
                            else {
                                var estrazione = _sessionLocator.For(typeof(PushNotificationRecord))
                                    .CreateSQLQuery(string.Format("select count(1) from ( {0} ) x where (x.Device = '{1}' or '{1}' = 'All') and x.Produzione = {2} and x.Validated = 1 and (x.Language = '{3}' or '{3}' = 'All') ", queryDevice, (locTipoDispositivo == null) ? "All" : locTipoDispositivo.ToString(), (produzione) ? 1 : 0, language))
                                 .UniqueResult();
                                counter = Convert.ToInt32(estrazione);
                            }
                        }
                        mpp.TargetDeviceNumber = counter;
                        _notifier.Information(T("Notification sent: " + messageSent.ToString()));
                        _myLog.WriteLog("Notification sent: " + messageSent.ToString());
                    }
                }
                string title = "no title";
                try {
                    title = ci.As<TitlePart>().Title;
                }
                catch { }
                _myLog.WriteLog("Terminato invio Push del content " + ci.Id + " " + title);
            }
            catch (Exception ex) {
                string title = "no title";
                try {
                    title = ci.As<TitlePart>().Title;
                }
                catch { }
                _myLog.WriteLog(ex.Message);
                _myLog.WriteLog("Errore  invio Push del content " + ci.Id + " " + title);
            }
        }

        #region Send push to Devices

        private string[] getextrainfo(Int32 id_of_content) {
            string[] extrainfo = new string[] { "", "" };
            try {
                var thecontent = ((ContentItem)_orchardServices.ContentManager.Get(id_of_content));
                var theautoroute = thecontent.As<AutoroutePart>();
                extrainfo[0] = thecontent.ContentType;
                extrainfo[1] = theautoroute.DisplayAlias;
            }
            catch { }

            return extrainfo;
        }

        private void SendAllAndroidPart(MobilePushPart mpp, Int32 idcontent, Int32 idContentRelated, string language, bool produzione, string queryDevice, int[] queryIds) {
            PushAndroidVM newpush = new PushAndroidVM();
            if (mpp.ContentItem.ContentType == "CommunicationAdvertising") {
                SendAllAdvertisingAndroid(mpp, idContentRelated, language, queryDevice, produzione, queryIds);
            }
            else {
                string ctype = "";
                string displayalias = "";
                var extra = getextrainfo(idContentRelated > 0 ? idContentRelated : idcontent);
                ctype = extra[0];
                displayalias = extra[1];
                newpush = GenerateAndroidPush(mpp, idcontent, idContentRelated, ctype, displayalias);
                SendAllAndroid(mpp.ContentItem.ContentType, newpush, produzione, language, queryDevice, queryIds);
            }
        }

        private void SendAllAdvertisingAndroid(MobilePushPart mpp, int idContentRelated, string language, string queryDevice, bool produzione, int[] queryIds) {
            Dictionary<string, string> pushexternal = new Dictionary<string, string>();
            pushexternal.Add("Text", mpp.TextPush);

            if (idContentRelated > 0) {
                pushexternal.Add("Iu", idContentRelated.ToString());
            }
            else if (!string.IsNullOrEmpty(((dynamic)(mpp.ContentItem.As<CommunicationAdvertisingPart>())).UrlLinked.Value)) {
                string shortlink = _communicationService.GetCampaignLink("Push", mpp);
                pushexternal.Add("Eu", shortlink);
            }
            string message = JsonConvert.SerializeObject(pushexternal);
            SendAllAndroidJson(mpp.ContentItem.Id, mpp.ContentItem.ContentType, message, produzione, language, queryDevice, queryIds);
        }

        private void SendAllAndroid(string contenttype, PushAndroidVM newpush, bool produzione, string language, string queryDevice = "", int[] queryIds = null) {
            string message = JsonConvert.SerializeObject(newpush);
            SendAllAndroidJson(newpush.Id, contenttype, message, produzione, language, queryDevice, queryIds);
        }

        private string GetQueryDevice(Dictionary<string, object> contesto, MobilePushPart mpp) {
            string withtoken = mpp.Settings.GetModel<PushMobilePartSettingVM>().QueryDevice;
            if (string.IsNullOrEmpty(withtoken))
                return "";
            else
                return _tokenizer.Replace(withtoken.Replace("\r\n", " ").Replace("\t", " "), contesto);
        }

        private List<PushNotificationVM> GetListMobileDevice(string contenttype, string queryDevice, TipoDispositivo tipodisp, bool produzione, string language, int[] queryIds) {
            var lista = new List<PushNotificationVM>();
            try {
                if (contenttype == "CommunicationAdvertising") {
                    var elenco = GetPushQueryResult(queryIds, tipodisp, produzione, language);
                    foreach (Hashtable ht in elenco) {
                        lista.Add(new PushNotificationVM {
                            Id = Convert.ToInt32(ht["Id"]),
                            Device = (TipoDispositivo)(Enum.Parse(typeof(TipoDispositivo), ht["Device"].ToString())),
                            Produzione = Convert.ToBoolean(ht["Produzione"], CultureInfo.InvariantCulture),
                            Validated = Convert.ToBoolean(ht["Validated"], CultureInfo.InvariantCulture),
                            Language = ht["Language"].ToString(),
                            UUIdentifier = ht["UUIdentifier"].ToString(),
                            Token = ht["Token"].ToString()
                        });
                    }
                }
                else {
                    IEnumerable<PushNotificationRecord> elenco = new List<PushNotificationRecord>();
                    if (queryDevice.Trim() == "") {
                        elenco = _pushNotificationRepository.Fetch(x => x.Device == tipodisp && x.Produzione == produzione && x.Validated == true && (x.Language == language || language == "All"));
                        foreach (PushNotificationRecord pnr in elenco) {
                            lista.Add(new PushNotificationVM {
                                Id = pnr.Id,
                                Device = pnr.Device,
                                Produzione = pnr.Produzione,
                                Validated = pnr.Validated,
                                Language = pnr.Language,
                                UUIdentifier = pnr.UUIdentifier,
                                Token = pnr.Token
                            });
                        }
                    }
                    else {
                        var estrazione = _sessionLocator.For(typeof(PushNotificationRecord))
                            .CreateSQLQuery(string.Format("select Id, Device, Produzione, Validated, Language, UUIdentifier, Token from ( {0} ) x where x.Device = '{1}' and x.Produzione = {2} and x.Validated = 1 and (x.Language = '{3}' or '{3}' = 'All') ", queryDevice, tipodisp, (produzione) ? 1 : 0, language))
                         .List();
                        object[] ht = null;
                        foreach (var arr in estrazione) {
                            ht = (object[])arr;
                            lista.Add(new PushNotificationVM {
                                Id = Convert.ToInt32(ht[0]),
                                Device = (TipoDispositivo)(Enum.Parse(typeof(TipoDispositivo), ht[1].ToString())),
                                Produzione = Convert.ToBoolean(ht[2], CultureInfo.InvariantCulture),
                                Validated = Convert.ToBoolean(ht[3], CultureInfo.InvariantCulture),
                                Language = ht[4].ToString(),
                                UUIdentifier = ht[5].ToString(),
                                Token = ht[6].ToString()
                            });
                        }
                    }
                }
            }
            catch (Exception ex) {
                _myLog.WriteLog(string.Format("Error in PushNotificationService.GetListMobileDevice(): {0} - {1}", ex.Message, ex.StackTrace));
            }
            return lista;
        }

        private void SendAllAndroidJson(Int32 idcontenttopush, string contenttype, string JsonMessage, bool produzione, string language, string queryDevice = "", int[] queryIds = null) {
            var allDevice = GetListMobileDevice(contenttype, queryDevice, TipoDispositivo.Android, produzione, language, queryIds);
            allDevice = RemoveSent(allDevice, idcontenttopush);
            string setting = "";
            if (produzione)
                setting = _orchardServices.WorkContext.CurrentSite.As<PushMobileSettingsPart>().AndroidApiKey;
            else
                setting = _orchardServices.WorkContext.CurrentSite.As<PushMobileSettingsPart>().AndroidApiKeyDevelopment;
            var config = new GcmConfiguration(setting);
            var serviceUrl = _orchardServices.WorkContext.CurrentSite.As<PushMobileSettingsPart>().AndroidPushServiceUrl;
            if (string.IsNullOrWhiteSpace(serviceUrl) == false) {
                config.OverrideUrl(serviceUrl);
            }
            var push = new GcmServiceBroker(config);
            push.OnNotificationSucceeded += (notification) => {
                NotificationSent(notification);
            };
            push.OnNotificationFailed += (notification, aggregateEx) => {
                aggregateEx.Handle(ex => {
                    if (ex is DeviceSubscriptionExpiredException) {
                        var expiredException = (DeviceSubscriptionExpiredException)ex;

                        var oldId = expiredException.OldSubscriptionId;
                        var newId = expiredException.NewSubscriptionId;

                        if (!string.IsNullOrWhiteSpace(newId)) {
                            // If this value isn't null, our subscription changed
                            DeviceSubscriptionChangedAndroid(notification.GetType().Name, oldId, newId, expiredException.Notification);
                        }
                        else if (produzione) {
                            DeviceSubscriptionExpiredAndroidProduzione(notification.GetType().Name, oldId, expiredException.ExpiredAt, expiredException.Notification);
                        }
                        else {
                            DeviceSubscriptionExpiredAndroidNotProduzione(notification.GetType().Name, oldId, expiredException.ExpiredAt, expiredException.Notification);
                        }
                    }
                    else {
                        NotificationFailed(notification, aggregateEx);
                    }

                    // Mark it as handled
                    return true;
                });
            };
            push.Start();
            foreach (PushNotificationVM pnr in allDevice) {
                push.QueueNotification(new GcmNotification {
                    RegistrationIds = new List<string> { pnr.Token },
                    Data = JObject.Parse(JsonMessage)
                });

                if (idcontenttopush > 0) {
                    SentRecord sr = new SentRecord();
                    sr.DeviceType = "Android";
                    sr.PushNotificationRecord_Id = pnr.Id;
                    sr.PushedItem = idcontenttopush;
                    sr.SentDate = DateTime.UtcNow;
                    _sentRepository.Create(sr);
                    _sentRepository.Flush();
                }
            }
            push.Stop();
        }

        private void SendAllApplePart(MobilePushPart mpp, Int32 idcontent, Int32 idContentRelated, string language, bool produzione, string queryDevice, int[] queryIds) {
            string ctype = "";
            string displayalias = "";
            var extra = getextrainfo(idContentRelated > 0 ? idContentRelated : idcontent);
            ctype = extra[0];
            displayalias = extra[1];
            PushAppleVM newpush = new PushAppleVM();
            newpush = GenerateApplePush(mpp, idcontent, idContentRelated, ctype, displayalias);
            SendAllApple(mpp.ContentItem.ContentType, newpush, produzione, language, queryDevice, queryIds);
        }

        private void SendAllApple(string contenttype, PushAppleVM newpush, bool produzione, string language, string queryDevice = "", int[] queryIds = null) {
            var allDevice = GetListMobileDevice(contenttype, queryDevice, TipoDispositivo.Apple, produzione, language, queryIds);
            if (newpush.ValidPayload) {
                PushApple(allDevice, produzione, newpush);
            }
        }

        private void SendAllWindowsMobilePart(MobilePushPart mpp, Int32 idcontent, Int32 idContentRelated, string language, bool produzione, string queryDevice, int[] queryIds) {
            var winPush = GenerateWindowsMobilePush(mpp, idcontent, idContentRelated);
            string message = string.Format(@"
            <toast>
                <visual>
                    <binding template=""ToastGeneric"">
                        <text>{0}</text>
                    </binding>  
                </visual>
            </toast>", winPush.Text);
            SendAllWindowsMobile(mpp.ContentItem.ContentType, message, produzione, language, queryIds, queryDevice);
        }

        private void SendAllWindowsMobile(string contenttype, string message, bool produzione, string language, int[] queryIds, string queryDevice = "") {
            var allDevice = GetListMobileDevice(contenttype, queryDevice, TipoDispositivo.WindowsMobile, produzione, language, queryIds);
            var setting_WindowsAppPackageName = _orchardServices.WorkContext.CurrentSite.As<PushMobileSettingsPart>().WindowsAppPackageName;
            var setting_WindowsAppSecurityIdentifier = _orchardServices.WorkContext.CurrentSite.As<PushMobileSettingsPart>().WindowsAppSecurityIdentifier;
            var setting_WindowsEndPoint = _orchardServices.WorkContext.CurrentSite.As<PushMobileSettingsPart>().WindowsEndPoint;
            var config = new WnsConfiguration(setting_WindowsAppPackageName, setting_WindowsAppSecurityIdentifier, setting_WindowsEndPoint);
            var push = new WnsServiceBroker(config);
            push.OnNotificationSucceeded += (notification) => {
            };
            push.OnNotificationFailed += (notification, aggregateEx) => {
                aggregateEx.Handle(ex => {
                    if (ex is DeviceSubscriptionExpiredException) {
                        var expiredException = (DeviceSubscriptionExpiredException)ex;

                        var oldId = expiredException.OldSubscriptionId;
                        var newId = expiredException.NewSubscriptionId;

                        if (!string.IsNullOrWhiteSpace(newId)) {
                            // If this value isn't null, our subscription changed
                            DeviceSubscriptionChangedWindows(notification.GetType().Name, oldId, newId, expiredException.Notification);
                        }
                        else if (produzione) {
                            DeviceSubscriptionExpired(notification.GetType().Name, oldId, expiredException.ExpiredAt, expiredException.Notification, produzione, TipoDispositivo.WindowsMobile);
                        }
                        else {
                            DeviceSubscriptionExpired(notification.GetType().Name, oldId, expiredException.ExpiredAt, expiredException.Notification, produzione, TipoDispositivo.WindowsMobile);
                        }
                    }
                    else {
                        NotificationFailed(notification, aggregateEx);
                    }
                    // Mark it as handled
                    return true;
                });
            };

            // TODO: da gestire produzione

            push.Start();
            foreach (PushNotificationVM pnr in allDevice) {
                push.QueueNotification(new WnsToastNotification {
                    ChannelUri = pnr.Token,
                    Payload = XElement.Parse(message)
                });
            }
            push.Stop();
        }

        private PushAndroidVM GenerateAndroidPush(MobilePushPart mpp, Int32 idcontent, Int32 idContentRelated, string ctype, string displayalias) {
            PushAndroidVM mypush = new PushAndroidVM();
            mypush.Text = mpp.TextPush;
            mypush.Id = idcontent;
            mypush.Rid = idContentRelated;
            mypush.Ct = ctype;
            mypush.Al = displayalias;
            return mypush;
        }

        private PushAppleVM GenerateApplePush(MobilePushPart mpp, Int32 idcontent, Int32 idContentRelated, string ctype, string displayalias) {
            StringBuilder sb = new StringBuilder();
            PushAppleVM mypush = new PushAppleVM();
            mypush.Title = mpp.TitlePush;
            mypush.Text = mpp.TextPush;
            mypush.idContent = idcontent;
            mypush.idRelated = idContentRelated;
            mypush.Ct = ctype;
            mypush.Al = displayalias;
            mypush.ValidPayload = true;
            if (mpp.ContentItem.ContentType == "CommunicationAdvertising") {
                string chiave = "";
                string valore = "";
                if (!string.IsNullOrEmpty(((dynamic)(mpp.ContentItem.As<CommunicationAdvertisingPart>())).UrlLinked.Value)) {
                    chiave = "Eu";
                    valore = _communicationService.GetCampaignLink("Push", mpp);
                }
                else {
                    chiave = "Iu";
                    valore = mpp.ContentItem.Id.ToString();
                }
                sb.AppendFormat("{{ \"aps\": {{ \"alert\": \"{0}\", \"sound\":\"{1}\"}}", FormatJsonValue(mypush.Text), FormatJsonValue(mypush.Sound));
                sb.AppendFormat(",\"{0}\":\"{1}\"", FormatJsonValue(chiave), FormatJsonValue(valore));
                sb.Append("}");
            }
            else {
                var partSettings = mpp.Settings.GetModel<PushMobilePartSettingVM>();
                if (!(partSettings.AcceptZeroRelated) && mypush.idRelated == 0) {
                    mypush.idRelated = mypush.idContent;
                }
                sb.AppendFormat("{{ \"aps\": {{ \"alert\": \"{0}\", \"sound\":\"{1}\"}}", FormatJsonValue(mypush.Text), FormatJsonValue(mypush.Sound));
                sb.AppendFormat(",\"Id\":{0}", mypush.idContent);
                sb.AppendFormat(",\"Rid\":{0}", mypush.idRelated);
                sb.AppendFormat(",\"Ct\":\"{0}\"", FormatJsonValue(mypush.Ct));
                sb.AppendFormat(",\"Al\":\"{0}\"", FormatJsonValue(mypush.Al));
                sb.Append("}");
            }
            if (mypush.Text.Length > MAX_PUSH_TEXT_LENGTH) {
                _notifier.Information(T("Sent: message payload exceed the limit"));
                _myLog.WriteLog("Sent: message payload exceed the limit");
                mypush.ValidPayload = false;
            }
            return mypush;
        }

        private PushWindowsMobileVM GenerateWindowsMobilePush(MobilePushPart mpp, Int32 idcontent, Int32 idContentRelated) {
            PushWindowsMobileVM mypush = new PushWindowsMobileVM();
            mypush.Title = mpp.TitlePush;
            mypush.Text = mpp.TextPush;
            mypush.idContent = idcontent;
            mypush.idRelated = idContentRelated;
            return mypush;
        }

        #endregion Send push to Devices

        private List<PushNotificationVM> RemoveSent(List<PushNotificationVM> listdispositivo, Int32 IdContent) {
            if (IdContent > 0) {
                List<Int32> listainvii = _sentRepository.Fetch(x => x.PushedItem == IdContent).Select(y => y.PushNotificationRecord_Id).ToList();
                return listdispositivo.Where(x => !listainvii.Contains(x.Id)).ToList();
            }
            else
                return listdispositivo;
        }
        private void PushApple(List<PushNotificationVM> listdispositivo, bool produzione, PushAppleVM pushMessage) {
            listdispositivo = RemoveSent(listdispositivo, pushMessage.idContent);
            if (string.IsNullOrWhiteSpace(pushMessage.Sound))
                pushMessage.Sound = _orchardServices.WorkContext.CurrentSite.As<PushMobileSettingsPart>().ApplePushSound;
            if (string.IsNullOrWhiteSpace(pushMessage.Sound))
                pushMessage.Sound = "sound.caf"; //default
            string setting_password = "";
            string setting_file = "";
            bool certificateexist = true;
            ApnsConfiguration.ApnsServerEnvironment environment = ApnsConfiguration.ApnsServerEnvironment.Sandbox;
            if (produzione) {
                environment = ApnsConfiguration.ApnsServerEnvironment.Production;
                setting_password = _orchardServices.WorkContext.CurrentSite.As<PushMobileSettingsPart>().AppleCertificatePassword;
                setting_file = HostingEnvironment.MapPath("~/") + @"App_Data\Sites\" + _shellSetting.Name + @"\Mobile\" + _orchardServices.WorkContext.CurrentSite.As<PushMobileSettingsPart>().ApplePathCertificateFile;
                if (string.IsNullOrEmpty(_orchardServices.WorkContext.CurrentSite.As<PushMobileSettingsPart>().ApplePathCertificateFile))
                    certificateexist = false;
            }
            else {
                setting_password = _orchardServices.WorkContext.CurrentSite.As<PushMobileSettingsPart>().AppleCertificatePasswordDevelopment;
                setting_file = HostingEnvironment.MapPath("~/") + @"App_Data\Sites\" + _shellSetting.Name + @"\Mobile\" + _orchardServices.WorkContext.CurrentSite.As<PushMobileSettingsPart>().ApplePathCertificateFileDevelopment;
                if (string.IsNullOrEmpty(_orchardServices.WorkContext.CurrentSite.As<PushMobileSettingsPart>().ApplePathCertificateFileDevelopment))
                    certificateexist = false;
            }
            if (certificateexist) {
                var config = new ApnsConfiguration(environment, setting_file, setting_password);
                var push = new ApnsServiceBroker(config);
                push.OnNotificationSucceeded += (notification) => {
                };
                push.OnNotificationFailed += (notification, aggregateEx) => {
                    aggregateEx.Handle(ex => {
                        if (ex is DeviceSubscriptionExpiredException) {
                            var expiredException = (DeviceSubscriptionExpiredException)ex;

                            var oldId = expiredException.OldSubscriptionId;
                            var newId = expiredException.NewSubscriptionId;

                            if (!string.IsNullOrWhiteSpace(newId)) {
                                // If this value isn't null, our subscription changed
                                DeviceSubscriptionChangedApple(notification.GetType().Name, oldId, newId, expiredException.Notification);
                            }
                            else if (produzione) {
                                DeviceSubscriptionExpiredAppleProduzione(notification.GetType().Name, oldId, expiredException.ExpiredAt, expiredException.Notification);
                            }
                            else {
                                DeviceSubscriptionExpiredAppleNotProduzione(notification.GetType().Name, oldId, expiredException.ExpiredAt, expiredException.Notification);
                            }
                        }
                        else {
                            NotificationFailed(notification, aggregateEx);
                        }
                        // Mark it as handled
                        return true;
                    });
                };

                StringBuilder sb = new StringBuilder();
                push.Start();
                foreach (PushNotificationVM dispositivo in listdispositivo) {
                    sb.Clear();
                    sb.AppendFormat("{{ \"aps\": {{ \"alert\": \"{0}\", \"sound\":\"{1}\"}}", FormatJsonValue(pushMessage.Text), FormatJsonValue(pushMessage.Sound));
                    if (!string.IsNullOrEmpty(pushMessage.Eu)) {
                        sb.AppendFormat(",\"Eu\":\"{0}\"", FormatJsonValue(pushMessage.Eu));
                    }
                    else if (!string.IsNullOrEmpty(pushMessage.Iu)) {
                        sb.AppendFormat(",\"Iu\":\"{0}\"", FormatJsonValue(pushMessage.Iu));
                    }
                    else {
                        sb.AppendFormat(",\"Id\":{0}", pushMessage.idContent);
                        sb.AppendFormat(",\"Rid\":{0}", pushMessage.idRelated);
                        sb.AppendFormat(",\"Ct\":\"{0}\"", FormatJsonValue(pushMessage.Ct));
                        sb.AppendFormat(",\"Al\":\"{0}\"", FormatJsonValue(pushMessage.Al));
                    }
                    sb.Append("}");
                    if (pushMessage.Text.Length > MAX_PUSH_TEXT_LENGTH) {
                        _notifier.Information(T("Sent: message payload exceed the limit"));
                        _myLog.WriteLog("Sent: message payload exceed the limit");
                    }
                    else {
                        push.QueueNotification(new ApnsNotification {
                            DeviceToken = dispositivo.Token,
                            Payload = JObject.Parse(sb.ToString())
                        });

                        if (pushMessage.idContent > 0) {
                            SentRecord sr = new SentRecord();
                            sr.DeviceType = "Apple";
                            sr.PushNotificationRecord_Id = dispositivo.Id;
                            sr.PushedItem = pushMessage.idContent;
                            sr.SentDate = DateTime.UtcNow;
                            _sentRepository.Create(sr);
                            _sentRepository.Flush();
                        }
                    }
                }
                push.Stop();
            }
        }

        private string FormatJsonValue(string text) {
            return (text ?? "").Replace("\"", "\\\"").Replace("\\", "\\\\");
        }

        private void DeviceSubscriptionChangedAndroid(object sender, string oldSubscriptionId, string newSubscriptionId, INotification notification) {
            DeviceSubscriptionChanged(sender, oldSubscriptionId, newSubscriptionId, notification, TipoDispositivo.Android);
        }

        private void DeviceSubscriptionChangedApple(object sender, string oldSubscriptionId, string newSubscriptionId, INotification notification) {
            DeviceSubscriptionChanged(sender, oldSubscriptionId, newSubscriptionId, notification, TipoDispositivo.Apple);
        }

        private void DeviceSubscriptionChangedWindows(object sender, string oldSubscriptionId, string newSubscriptionId, INotification notification) {
            DeviceSubscriptionChanged(sender, oldSubscriptionId, newSubscriptionId, notification, TipoDispositivo.WindowsMobile);
        }

        private void DeviceSubscriptionChanged(object sender, string oldSubscriptionId, string newSubscriptionId, INotification notification, TipoDispositivo tipoDispositivo) {
            _myLog.WriteLog(T("Device Registration Changed:  Old-> " + oldSubscriptionId + "  New-> " + newSubscriptionId + " -> " + notification).ToString());
            PushNotificationRecord pnr = _pushNotificationRepository.Fetch(x => x.Token == oldSubscriptionId && x.Device == tipoDispositivo).FirstOrDefault();
            IEnumerable<PushNotificationRecord> esiste_il_nuovo = _pushNotificationRepository.Fetch(x => x.Token == newSubscriptionId && x.Device == tipoDispositivo);
            if (esiste_il_nuovo != null && esiste_il_nuovo.FirstOrDefault() != null)
                pnr.Validated = false;
            else
                pnr.Token = newSubscriptionId;
            _pushNotificationRepository.Update(pnr);
            _pushNotificationRepository.Flush();
        }

        private void NotificationSent(INotification notification) {
            if (notification is ApnsNotification) {
                _myLog.WriteLog(T("Sent: " + notification.GetType().Name + " -> " + (notification as ApnsNotification).DeviceToken + " -> " + notification).ToString());
            }
            else {
                _myLog.WriteLog(T("Sent: " + notification.GetType().Name + " -> " + notification).ToString());
            }
            messageSent++;
        }

        private void NotificationFailed(INotification notification, AggregateException notificationFailureException) {
            _myLog.WriteLog((T("Failure: " + notification.GetType().Name + " -> " + notificationFailureException.Message + " -> " + notification.ToString())).ToString());
        }

        private void DeviceSubscriptionExpiredAppleProduzione(object sender, string expiredDeviceSubscriptionId, DateTime timestamp, INotification notification) {
            DeviceSubscriptionExpired(sender, expiredDeviceSubscriptionId, timestamp, notification, true, TipoDispositivo.Apple);
        }

        private void DeviceSubscriptionExpiredAppleNotProduzione(object sender, string expiredDeviceSubscriptionId, DateTime timestamp, INotification notification) {
            DeviceSubscriptionExpired(sender, expiredDeviceSubscriptionId, timestamp, notification, false, TipoDispositivo.Apple);
        }

        private void DeviceSubscriptionExpiredAndroidProduzione(object sender, string expiredDeviceSubscriptionId, DateTime timestamp, INotification notification) {
            DeviceSubscriptionExpired(sender, expiredDeviceSubscriptionId, timestamp, notification, true, TipoDispositivo.Android);
        }

        private void DeviceSubscriptionExpiredAndroidNotProduzione(object sender, string expiredDeviceSubscriptionId, DateTime timestamp, INotification notification) {
            DeviceSubscriptionExpired(sender, expiredDeviceSubscriptionId, timestamp, notification, false, TipoDispositivo.Android);
        }

        private void DeviceSubscriptionExpired(object sender, string expiredDeviceSubscriptionId, DateTime timestamp, INotification notification, bool produzione, TipoDispositivo dispositivo) {
            _myLog.WriteLog(T("Device Subscription Expired: " + sender + " -> " + expiredDeviceSubscriptionId).ToString());
            if (_pushNotificationRepository.Fetch(x => x.Token == expiredDeviceSubscriptionId && x.Produzione == produzione && x.Device == dispositivo).Count() == 1) {
                PushNotificationRecord pnr = _pushNotificationRepository.Fetch(x => x.Token == expiredDeviceSubscriptionId && x.Produzione == produzione && x.Device == dispositivo).FirstOrDefault();
                pnr.Validated = false;
                _pushNotificationRepository.Update(pnr);
                _pushNotificationRepository.Flush();
                _myLog.WriteLog(T("Device Subscription Expired Action: " + sender + " not validated -> " + expiredDeviceSubscriptionId).ToString());
            }
            else {
                _myLog.WriteLog(T("Device Subscription Expired Error: " + sender + " -> token not found or token not unique:" + expiredDeviceSubscriptionId).ToString());
            }
        }

        private void DeviceSubscriptionExpiredWindowsMobile(object sender, string expiredDeviceSubscriptionId, DateTime timestamp, INotification notification) {
            _myLog.WriteLog(T("Device Subscription Expired: " + sender + " -> " + expiredDeviceSubscriptionId).ToString());
            // ToDo
            _myLog.WriteLog(T("The event is not implemented for Windows").ToString());
        }
    }
}