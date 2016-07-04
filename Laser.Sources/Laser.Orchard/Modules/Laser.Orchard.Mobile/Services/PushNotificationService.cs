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
using PushSharp.Android;
using PushSharp.Apple;
using PushSharp.Core;
using PushSharp.Windows;
using PushSharp.WindowsPhone;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Hosting;

namespace Laser.Orchard.Mobile.Services {

    public interface IPushNotificationService : IDependency {

        IList GetPushQueryResult(Int32[] ids, bool countOnly = false);
        IList GetPushQueryResult(Int32[] ids, TipoDispositivo? tipodisp, bool produzione, string language, bool countOnly = false);
        void StorePushNotification(PushNotificationRecord pushElement);

        IEnumerable<PushNotificationRecord> SearchPushNotification(string texttosearch);

        //  void SendPush(Int32 iddispositivo, string message);
        //void PublishedPushEvent(dynamic mycontext, ContentItem ci);
        void PublishedPushEvent(ContentItem ci);

        void SendPushService(bool produzione, string device, Int32 idContentRelated, string language_param, string messageApple, string messageAndroid, string JsonAndroid, string messageWindows, string sound, string queryDevice = "");

        void Synchronize();
    }

    public class PushNotificationService : IPushNotificationService {
        private readonly IRepository<SentRecord> _sentRepository;
        private readonly IRepository<PushNotificationRecord> _pushNotificationRepository;
        private readonly IRepository<UserDeviceRecord> _userDeviceRecord;
        //private readonly IRepository<CommunicationSmsRecord> _repositoryCommunicationSmsRecord;
        private readonly IQueryPickerService _queryPickerServices;

        public Localizer T { get; set; }
        private readonly INotifier _notifier;
        private readonly IOrchardServices _orchardServices;
        private readonly IMylogService _myLog;
        private readonly ShellSettings _shellSetting;
        private readonly ISessionLocator _sessionLocator;
        public ICommunicationService _communicationService;
        private readonly ITokenizer _tokenizer;
        private Int32 messageSent;

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
                IQueryPickerService queryPickerService
            //   IRepository<CommunicationSmsRecord> repositoryCommunicationSmsRecord,
            //      ICommunicationService communicationService non posso usare l'injection altrimenti vanno in errore i tenant che non hanno ancora la communication abilitata
         ) {
            //      _repositoryCommunicationSmsRecord = repositoryCommunicationSmsRecord;
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
            //    _communicationService = communicationService;
            _orchardServices.WorkContext.TryResolve<ICommunicationService>(out _communicationService);
            _queryPickerServices = queryPickerService;
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
                //queryForPush += " AND civr.Id in (" + stringHQL + ")";
            }
            // x.Device == tipodisp && x.Produzione == produzione && x.Validated == true && (x.Language == language || language == "All")

            // Creo query ottimizzata per le performance
            var fullStatement = _sessionLocator.For(null)
                .CreateQuery(queryForPush)
                .SetCacheable(false);

            //IList lista = fullStatement
            //        .SetResultTransformer(Transformers.AliasToEntityMap)
            //        .List();
            //return lista;
            var lista = fullStatement
                .SetResultTransformer(Transformers.AliasToEntityMap)  // (Transformers.AliasToBean<PushNotificationRecord>())
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

        public void Synchronize() {
            //#region lego tutti gli sms
            //var alluser = _orchardServices.ContentManager.Query<UserPart, UserPartRecord>().Where(x=>x.RegistrationStatus==UserStatus.Approved);
            //if (alluser.List().FirstOrDefault().As<UserPwdRecoveryPart>() != null) {
            //    var allusercol=alluser.List().Where(x=>!string.IsNullOrEmpty(x.ContentItem.As<UserPwdRecoveryPart>().PhoneNumber)).ToList();
            //    foreach (IContent user in allusercol) {
            //        string pref = user.ContentItem.As<UserPwdRecoveryPart>().InternationalPrefix;
            //        string num = user.ContentItem.As<UserPwdRecoveryPart>().PhoneNumber;
            //        CommunicationSmsRecord csr = _repositoryCommunicationSmsRecord.Fetch(x => x.Sms == num && x.Prefix == pref).FirstOrDefault();
            //        CommunicationContactPart ciCommunication = _orchardServices.ContentManager.Query<CommunicationContactPart, CommunicationContactPartRecord>().Where(x => x.UserPartRecord_Id == user.Id).List().FirstOrDefault();
            //        if (ciCommunication == null) {
            //            // Una contact part dovrebbe esserci in quanto questo codice viene eseguito dopo la sincronizzazione utenti
            //            // Se non vi è una contartpart deduco che il dato sia sporco (es: UUid di un utente che è stato cancellato quindi non sincronizzo il dato con contactpart, verrà legato come se fosse scollegato al contentitem che raggruppa tutti i scollegati)
            //            //throw new Exception("Utente senza associazione alla profilazione");
            //        }
            //        else {
            //            if (csr == null) {
            //                CommunicationSmsRecord newsms = new CommunicationSmsRecord();
            //                newsms.Prefix = pref;
            //                newsms.Sms = num;
            //                newsms.CommunicationContactPartRecord_Id = ciCommunication.ContentItem.Id;
            //                _repositoryCommunicationSmsRecord.Create(newsms);
            //                _repositoryCommunicationSmsRecord.Flush();
            //            }
            //            else {
            //                if (csr.CommunicationContactPartRecord_Id != ciCommunication.ContentItem.Id) {
            //                    csr.CommunicationContactPartRecord_Id = ciCommunication.ContentItem.Id;
            //                    csr.DataModifica = DateTime.Now;
            //                    _repositoryCommunicationSmsRecord.Update(csr);
            //                    _repositoryCommunicationSmsRecord.Flush();
            //                }
            //            }
            //        }
            //    }
            //}
            //#endregion

            List<UserDeviceRecord> lUdr = _userDeviceRecord.Fetch(x => x.UserPartRecord.Id > 0).ToList();
            foreach (UserDeviceRecord up in lUdr) {
                CommunicationContactPart ciCommunication = _orchardServices.ContentManager.Query<CommunicationContactPart, CommunicationContactPartRecord>().Where(x => x.UserPartRecord_Id == up.UserPartRecord.Id).List().FirstOrDefault();
                //  _communicationService.GetContactFromUser(up.UserPartRecord.Id);
                if (ciCommunication == null) {
                    // Una contact part dovrebbe esserci in quanto questo codice viene eseguito dopo la sincronizzazione utenti
                    // Se non vi è una contartpart deduco che il dato sia sporco (es: UUid di un utente che è stato cancellato quindi non sincronizzo il dato con contactpart, verrà legato come se fosse scollegato al contentitem che raggruppa tutti i scollegati)
                    //throw new Exception("Utente senza associazione alla profilazione");
                }
                else {
                    int idci = ciCommunication.ContentItem.Id;
                    var records = _pushNotificationRepository.Fetch(x => x.UUIdentifier == up.UUIdentifier).ToList();
                    foreach (PushNotificationRecord rec in records) {
                        rec.MobileContactPartRecord_Id = idci;
                    }
                    _pushNotificationRepository.Flush();
                }
            }
            _notifier.Add(NotifyType.Information, T("Linked {0} user's mobile device", lUdr.Count().ToString()));

            #region [lego i rimanenti content al Content Master per renderli querabili]

            //if (_orchardServices.ContentManager.Query<CommunicationContactPart, CommunicationContactPartRecord>().Where(y => y.Master).Count() == 0) {
            //    var Contact = _orchardServices.ContentManager.New("CommunicationContact");
            //    _orchardServices.ContentManager.Create(Contact);
            //    Contact.As<TitlePart>().Title = "Master Contact";
            //    Contact.As<CommunicationContactPart>().Master = true;
            //}
            //CommunicationContactPart master = _orchardServices.ContentManager.Query<CommunicationContactPart, CommunicationContactPartRecord>().Where(y => y.Master).List().FirstOrDefault();
            CommunicationContactPart master = _communicationService.EnsureMasterContact();
            int idmaster = master.Id;
            var notificationrecords = _pushNotificationRepository.Fetch(x => x.MobileContactPartRecord_Id == 0 || x.MobileContactPartRecord_Id == null).ToList();
            foreach (PushNotificationRecord rec in notificationrecords) {
                rec.MobileContactPartRecord_Id = idmaster;
            }
            _pushNotificationRepository.Flush();
            _notifier.Add(NotifyType.Information, T("Linked {0} device To Master contact", notificationrecords.Count().ToString()));

            #endregion [lego i rimanenti content al Content Master per renderli querabili]
        }

        #region [CRUD PushNotification]

        public void StorePushNotification(PushNotificationRecord pushElement) {
            PushNotificationRecord OldPush = _pushNotificationRepository.Fetch(x => (x.UUIdentifier == pushElement.UUIdentifier || x.Token == pushElement.Token) && x.Produzione == pushElement.Produzione && x.Device == pushElement.Device).FirstOrDefault();

            DateTime adesso = DateTime.Now;
            // PushNotificationRecord OldPush = GetPushNotificationBy_UUIdentifier(pushElement.UUIdentifier, pushElement.Produzione);
            if (OldPush != null && !string.IsNullOrEmpty(OldPush.UUIdentifier)) { // se dispositivo già registrato sovrascrivo lo stesso record
                if (OldPush.UUIdentifier != pushElement.UUIdentifier) {
                    if (_userDeviceRecord.Fetch(x => x.UUIdentifier == OldPush.UUIdentifier).Count() > 0) {
                        UserDeviceRecord my_disp = _userDeviceRecord.Fetch(x => x.UUIdentifier == OldPush.UUIdentifier).FirstOrDefault();
                        if (my_disp != null) {
                            my_disp.UUIdentifier = pushElement.UUIdentifier;
                            _userDeviceRecord.Update(my_disp);
                        }
                    }
                }
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
                _myLog.WriteLog(string.Format("TryGetContactId - Exception occurred: {0} \r\n    in {1}", ex.Message, ex.StackTrace));
            }
            return contactId;
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

        //public void PublishedPushEvent(dynamic mycontext, ContentItem ci) {
        public void PublishedPushEvent(ContentItem ci) {
            try {
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
                    //contentForPush = (dynamic)relatedContentItem;
                    idContentRelated = relatedContent.Ids[0];
                    relatedContentItem = _orchardServices.ContentManager.Get(idContentRelated);
                    if (relatedContentItem == null) {
                        relatedContentItem = _orchardServices.ContentManager.GetLatest(idContentRelated);
                    }
                    contentForPush = (dynamic)relatedContentItem;
                    if (!relatedContentItem.IsPublished()) {
                        _notifier.Information(T("No push will be sent, related content must be published"));
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
                    //mpp.ToPush = false;
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
                                //var estrazione = _sessionLocator.For(typeof(PushNotificationRecord))
                                // .CreateSQLQuery(queryDevice)
                                // .AddEntity(typeof(PushNotificationRecord))
                                // .List<PushNotificationRecord>();
                                //counter = estrazione.Where(x => (x.Device == locTipoDispositivo || locTipoDispositivo == null) && x.Produzione == produzione && x.Validated == true && (x.Language == language || language == "All")).Count();

                            var estrazione = _sessionLocator.For(typeof(PushNotificationRecord))
                                    .CreateSQLQuery(string.Format("select count(1) from ( {0} ) x where (x.Device = '{1}' or '{1}' = 'All') and x.Produzione = {2} and x.Validated = 1 and (x.Language = '{3}' or '{3}' = 'All') ", queryDevice, (locTipoDispositivo == null)? "All" : locTipoDispositivo.ToString(), (produzione) ? 1 : 0, language))
                                 .UniqueResult();
                                counter = Convert.ToInt32(estrazione);
                        }
                    }
                    mpp.TargetDeviceNumber = counter;
                    _notifier.Information(T("Notification sent: " + messageSent.ToString()));
                }
            }
               string title="no title";
                try{
                    title=ci.As<TitlePart>().Title;
        }
                catch{}
                _myLog.WriteLog("Terminato invio Push del content " + ci.Id + " " + title);
            }
            catch (Exception ex){
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

        //private void SendAllAdvertisingApple(MobilePushPart mpp, string language, string queryDevice, bool produzione) {
        //    Dictionary<string, string> pushexternal = new Dictionary<string, string>();
        //    pushexternal.Add("Text", mpp.TextPush);
        //    if (!string.IsNullOrEmpty(((dynamic)(mpp.ContentItem.As<CommunicationAdvertisingPart>())).UrlLinked.Value)) {
        //        string shortlink = _communicationService.GetCampaignLink("Push", mpp);
        //        pushexternal.Add("Eu", shortlink);
        //    }
        //    else {
        //        string comunicatoid = mpp.ContentItem.Id.ToString();
        //        pushexternal.Add("Iu", comunicatoid);
        //    }
        //    string message = JsonConvert.SerializeObject(pushexternal);
        //    SendAllAndroidJson(message, produzione, language, queryDevice);
        //}

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
            //else {
            //    string comunicatoid = mpp.ContentItem.Id.ToString();
            //    pushexternal.Add("Iu", comunicatoid);
            //}
            string message = JsonConvert.SerializeObject(pushexternal);
            SendAllAndroidJson(mpp.ContentItem.Id, mpp.ContentItem.ContentType, message, produzione, language, queryDevice, queryIds);
        }

        //todo remove
        //private void SendAllAndroid(PushAndroidVM newpush, bool produzione, string language, string queryDevice = "", int[] queryIds = null) {
        //    string message = JsonConvert.SerializeObject(newpush);
        //    SendAllAndroidJson(message, produzione, language, queryDevice, queryIds);
        //}
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

        // private IEnumerable<PushNotificationRecord> GetListMobileDevice(string queryDevice, TipoDispositivo tipodisp, bool produzione, string language, int[] queryIds) {

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
                         //.AddEntity(typeof(PushNotificationRecord))
                         .List();
                        //elenco = estrazione.Where(x => x.Device == tipodisp && x.Produzione == produzione && x.Validated == true && (x.Language == language || language == "All"));
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

        ////todo remove
        //private List<PushNotificationRecord> GetListMobileDevice(string queryDevice, TipoDispositivo tipodisp, bool produzione, string language, int[] queryIds) {
        //    if (queryDevice.Trim() == "") {
        //        var elenco = GetPushQueryResult(queryIds, tipodisp, produzione, language);
        //        var lista = new List<PushNotificationRecord>();
        //        foreach (Hashtable ht in elenco) {
        //            lista.Add(new PushNotificationRecord {
        //                Id = Convert.ToInt32(ht["Id"]),
        //                Device = (TipoDispositivo)(Enum.Parse(typeof(TipoDispositivo), ht["Device"].ToString())),
        //                Produzione = Convert.ToBoolean(ht["Produzione"], CultureInfo.InvariantCulture),
        //                Validated = Convert.ToBoolean(ht["Validated"], CultureInfo.InvariantCulture),
        //                Language = ht["Language"].ToString(),
        //                UUIdentifier = ht["UUIdentifier"].ToString(),
        //                Token = ht["Token"].ToString()
        //            });
        //        }
        //        return lista;
        //        //return _pushNotificationRepository.Fetch(x => x.Device == tipodisp && x.Produzione == produzione && x.Validated == true && (x.Language == language || language == "All"));
        //    }
        //    else {
        //        var estrazione = _sessionLocator.For(typeof(PushNotificationRecord))
        //         .CreateSQLQuery(queryDevice)
        //         .AddEntity(typeof(PushNotificationRecord))
        //         .List<PushNotificationRecord>();
        //        return estrazione.Where(x => x.Device == tipodisp && x.Produzione == produzione && x.Validated == true && (x.Language == language || language == "All")).ToList();
        //    }
        //}

        private void SendAllAndroidJson(Int32 idcontenttopush, string contenttype, string JsonMessage, bool produzione, string language, string queryDevice = "", int[] queryIds = null) {
            var allDevice = GetListMobileDevice(contenttype,queryDevice, TipoDispositivo.Android, produzione, language, queryIds);
            allDevice = RemoveSent(allDevice, idcontenttopush);
            string setting = "";
            if (produzione)
                setting = _orchardServices.WorkContext.CurrentSite.As<PushMobileSettingsPart>().AndroidApiKey;
            else
                setting = _orchardServices.WorkContext.CurrentSite.As<PushMobileSettingsPart>().AndroidApiKeyDevelopment;
            var push = new PushBroker();
            push.OnNotificationSent += NotificationSent;
            push.OnChannelException += ChannelException;
            push.OnServiceException += ServiceException;
            push.OnNotificationFailed += NotificationFailed;
            if (produzione)
                push.OnDeviceSubscriptionExpired += DeviceSubscriptionExpiredAndroidProduzione;
            else
                push.OnDeviceSubscriptionExpired += DeviceSubscriptionExpiredAndroidNotProduzione;
            push.OnDeviceSubscriptionChanged += DeviceSubscriptionChanged;
            push.OnChannelCreated += ChannelCreated;
            push.OnChannelDestroyed += ChannelDestroyed;

            push.RegisterGcmService(new GcmPushChannelSettings(setting));
            //Fluent construction of an Android GCM Notification
            //IMPORTANT: For Android you MUST use your own RegistrationId here that gets generated within your Android app itself!
    
            foreach (PushNotificationVM pnr in allDevice) {
                //  PushAndroid(pnr, produzione, JsonMessage);
                push.QueueNotification(new GcmNotification().ForDeviceRegistrationId(pnr.Token)
                .WithJson(JsonMessage));

                if (idcontenttopush>0){
                    SentRecord sr = new SentRecord();
                    sr.DeviceType="Android";
                    sr.PushNotificationRecord_Id=pnr.Id;
                    sr.PushedItem=idcontenttopush;
                    sr.SentDate=DateTime.UtcNow;
                    _sentRepository.Create(sr);
                    _sentRepository.Flush();
            }
            }
            push.StopAllServices();
        }

        ////todo remove
        //private void SendAllAndroidJson(string JsonMessage, bool produzione, string language, string queryDevice = "", int[] queryIds = null) {
        //    //var allDevice = _pushNotificationRepository.Fetch(x => x.Device == TipoDispositivo.Android && x.Produzione == produzione && x.Validated == true && (x.Language == language || language == "All"));
        //    var allDevice = GetListMobileDevice(queryDevice, TipoDispositivo.Android, produzione, language, queryIds);

        //    string setting = "";
        //    if (produzione)
        //        setting = _orchardServices.WorkContext.CurrentSite.As<PushMobileSettingsPart>().AndroidApiKey;
        //    else
        //        setting = _orchardServices.WorkContext.CurrentSite.As<PushMobileSettingsPart>().AndroidApiKeyDevelopment;
        //    var push = new PushBroker();
        //    push.OnNotificationSent += NotificationSent;
        //    push.OnChannelException += ChannelException;
        //    push.OnServiceException += ServiceException;
        //    push.OnNotificationFailed += NotificationFailed;
        //    if (produzione)
        //        push.OnDeviceSubscriptionExpired += DeviceSubscriptionExpiredAndroidProduzione;
        //    else
        //        push.OnDeviceSubscriptionExpired += DeviceSubscriptionExpiredAndroidNotProduzione;
        //    push.OnDeviceSubscriptionChanged += DeviceSubscriptionChanged;
        //    push.OnChannelCreated += ChannelCreated;
        //    push.OnChannelDestroyed += ChannelDestroyed;

        //    push.RegisterGcmService(new GcmPushChannelSettings(setting));
        //    //Fluent construction of an Android GCM Notification
        //    //IMPORTANT: For Android you MUST use your own RegistrationId here that gets generated within your Android app itself!
        //    foreach (PushNotificationRecord pnr in allDevice) {
        //        //  PushAndroid(pnr, produzione, JsonMessage);
        //        push.QueueNotification(new GcmNotification().ForDeviceRegistrationId(pnr.Token)
        //        .WithJson(JsonMessage));
        //    }
        //    //   .WithJson(" {    \"tipo\": \"aio\",    \"id\": \"2\",    \"titolo\": \"ole\"  }"));
        //    push.StopAllServices();
        //}

        //private void PushAndroid(PushNotificationRecord dispositivo, bool produzione, string message) {
        //      string setting = "";
        //      if (produzione)
        //          setting = _orchardServices.WorkContext.CurrentSite.As<PushMobileSettingsPart>().AndroidApiKey;
        //      else
        //          setting = _orchardServices.WorkContext.CurrentSite.As<PushMobileSettingsPart>().AndroidApiKeyDevelopment;
        //      var push = new PushBroker();
        //      push.OnNotificationSent += NotificationSent;
        //      push.OnChannelException += ChannelException;
        //      push.OnServiceException += ServiceException;
        //      push.OnNotificationFailed += NotificationFailed;
        //      if (produzione)
        //          push.OnDeviceSubscriptionExpired += DeviceSubscriptionExpiredAndroidProduzione;
        //      else
        //          push.OnDeviceSubscriptionExpired += DeviceSubscriptionExpiredAndroidNotProduzione;
        //      push.OnDeviceSubscriptionChanged += DeviceSubscriptionChanged;
        //      push.OnChannelCreated += ChannelCreated;
        //      push.OnChannelDestroyed += ChannelDestroyed;

        //      push.RegisterGcmService(new GcmPushChannelSettings(setting));
        //      //Fluent construction of an Android GCM Notification
        //      //IMPORTANT: For Android you MUST use your own RegistrationId here that gets generated within your Android app itself!

        //      push.QueueNotification(new GcmNotification().ForDeviceRegistrationId(dispositivo.Token)
        //          .WithJson(message));
        //      //   .WithJson(" {    \"tipo\": \"aio\",    \"id\": \"2\",    \"titolo\": \"ole\"  }"));

        //      push.StopAllServices();
        //  }

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
            //   var allDevice = _pushNotificationRepository.Fetch(x => x.Device == TipoDispositivo.Apple && x.Produzione == produzione && x.Validated == true && (x.Language == language || language == "All"));
            // PushAppleVM testpayloadsize = GenerateApplePush(mpp, idcontent, idContentRelated);
            if (newpush.ValidPayload) {
                //PushApple(allDevice.ToList(), produzione, newpush);
                PushApple(allDevice, produzione, newpush);
            }
        }

        private void SendAllWindowsMobilePart(MobilePushPart mpp, Int32 idcontent, Int32 idContentRelated, string language, bool produzione, string queryDevice, int[] queryIds) {
            string message = JsonConvert.SerializeObject(GenerateWindowsMobilePush(mpp, idcontent, idContentRelated));
            SendAllWindowsMobile(mpp.ContentItem.ContentType, message, produzione, language, queryIds, queryDevice);
        }

        private void SendAllWindowsMobile(string contenttype, string message, bool produzione, string language, int[] queryIds, string queryDevice = "") {
            var allDevice = GetListMobileDevice(contenttype, queryDevice, TipoDispositivo.WindowsMobile, produzione, language, queryIds);
            //var allDevice = _pushNotificationRepository.Fetch(x => x.Device == TipoDispositivo.WindowsMobile && x.Produzione == produzione && x.Validated == true && (x.Language == language || language == "All"));
            foreach (PushNotificationVM pnr in allDevice) {
                PushWindowsMobile(pnr, produzione, message);
            }
        }

        private PushAndroidVM GenerateAndroidPush(MobilePushPart mpp, Int32 idcontent, Int32 idContentRelated, string ctype, string displayalias) {
            PushAndroidVM mypush = new PushAndroidVM();
            //mypush.Title = mpp.TitlePush;
            mypush.Text = mpp.TextPush;
            mypush.Id = idcontent;
            mypush.Rid = idContentRelated;
            mypush.Ct = ctype;
            mypush.Al = displayalias;
            return mypush;
        }

        private PushAppleVM GenerateApplePush(MobilePushPart mpp, Int32 idcontent, Int32 idContentRelated, string ctype, string displayalias) {
            AppleNotification appleNotification = new AppleNotification();
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
                appleNotification.ForDeviceToken("TokenVirtualePerTestSuPayloadDimension")
                    .WithAlert(mypush.Text)
                    .WithCustomItem(chiave, valore)
                    .WithSound(mypush.Sound);
            }
            else {
                var partSettings = mpp.Settings.GetModel<PushMobilePartSettingVM>();
                if (!(partSettings.AcceptZeroRelated) && mypush.idRelated == 0)
                    mypush.idRelated = mypush.idContent;
                appleNotification.ForDeviceToken("TokenVirtualePerTestSuPayloadDimension")
                .WithAlert(mypush.Text)
                    //        .WithCustomItem("Title", mypush.Title)
                .WithCustomItem("Id", mypush.idContent)
                .WithCustomItem("Rid", mypush.idRelated)
                .WithCustomItem("Ct", mypush.Ct)
                .WithCustomItem("Al", mypush.Al)
                .WithSound(mypush.Sound);
            }
            if (appleNotification.Payload.ToJson().Length > 255) {
                _notifier.Information(T("Sent: message payload exceed the limit"));
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

        //public void SendPush(Int32 iddispositivo, string message) {//,string language,bool produzione
        //    PushNotificationRecord devicetopush = _pushNotificationRepository.Fetch(x => x.Id == iddispositivo).FirstOrDefault();
        //    if (devicetopush != null && devicetopush.Validated) {
        //        switch (devicetopush.Device) {
        //            case TipoDispositivo.Android:
        //                PushAndroid(devicetopush, message);
        //                break;
        //            case TipoDispositivo.Apple:

        //                PushApple(devicetopush, message);
        //                break;
        //            case TipoDispositivo.WindowsMobile:
        //                PushWindowsMobile(devicetopush, message);
        //                break;
        //        }
        //    }

        //}

        private void PushWindowsMobile(PushNotificationVM dispositivo, bool produzione, string message) {
            var setting_WindowsAppPackageName = _orchardServices.WorkContext.CurrentSite.As<PushMobileSettingsPart>().WindowsAppPackageName;
            var setting_WindowsAppSecurityIdentifier = _orchardServices.WorkContext.CurrentSite.As<PushMobileSettingsPart>().WindowsAppSecurityIdentifier;
            var setting_WindowsEndPoint = _orchardServices.WorkContext.CurrentSite.As<PushMobileSettingsPart>().WindowsEndPoint;
            var push = new PushBroker();
            push.OnNotificationSent += NotificationSent;
            push.OnChannelException += ChannelException;
            push.OnServiceException += ServiceException;
            push.OnNotificationFailed += NotificationFailed;
            push.OnDeviceSubscriptionExpired += DeviceSubscriptionExpiredWindowsMobile;
            push.OnDeviceSubscriptionChanged += DeviceSubscriptionChanged;
            push.OnChannelCreated += ChannelCreated;
            push.OnChannelDestroyed += ChannelDestroyed;
            // todo: da gestire produzione

            //Fluent construction of a Windows Phone Toast notification
            //IMPORTANT: For Windows Phone you MUST use your own Endpoint Uri here that gets generated within your Windows Phone app itself!
            push.QueueNotification(new WindowsPhoneToastNotification()
                .ForEndpointUri(new Uri(setting_WindowsEndPoint))
                .ForOSVersion(WindowsPhoneDeviceOSVersion.MangoSevenPointFive)
                .WithBatchingInterval(BatchingInterval.Immediate)
                .WithNavigatePath("/MainPage.xaml")
                .WithText1(message));
            //  .WithText2("This is a Toast"));

            //-------------------------
            // WINDOWS NOTIFICATIONS
            //-------------------------
            //Configure and start Windows Notifications
            push.RegisterWindowsService(new WindowsPushChannelSettings(setting_WindowsAppPackageName,
                setting_WindowsAppSecurityIdentifier, dispositivo.Token));
            //Fluent construction of a Windows Toast Notification
            push.QueueNotification(new WindowsToastNotification()
                .AsToastText01(message)
                .ForChannelUri(setting_WindowsEndPoint));

            push.StopAllServices();
        }

        private List<PushNotificationVM> RemoveSent( List<PushNotificationVM> listdispositivo,Int32 IdContent) {
            if (IdContent > 0) {
                List<Int32> listainvii = _sentRepository.Fetch(x => x.PushedItem == IdContent).Select(y => y.PushNotificationRecord_Id).ToList();
                return listdispositivo.Where(x => !listainvii.Contains(x.Id)).ToList();
            }
            else
                return listdispositivo;
        }
        private void PushApple(List<PushNotificationVM> listdispositivo, bool produzione, PushAppleVM pushMessage) {
            listdispositivo = RemoveSent(listdispositivo, pushMessage.idContent);
            //  string AppleCertificateTenant = _shellSetting.Name;
            if (string.IsNullOrWhiteSpace(pushMessage.Sound))
                pushMessage.Sound = _orchardServices.WorkContext.CurrentSite.As<PushMobileSettingsPart>().ApplePushSound;
            if (string.IsNullOrWhiteSpace(pushMessage.Sound))
                pushMessage.Sound = "sound.caf"; //default
            string setting_password = "";
            string setting_file = "";
            bool certificateexist = true;
            if (produzione) {
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
                var push = new PushBroker();
                push.OnNotificationSent += NotificationSent;
                push.OnChannelException += ChannelException;
                push.OnServiceException += ServiceException;
                push.OnNotificationFailed += NotificationFailed;
                if (produzione)
                    push.OnDeviceSubscriptionExpired += DeviceSubscriptionExpiredAppleProduzione;
                else
                    push.OnDeviceSubscriptionExpired += DeviceSubscriptionExpiredAppleNotProduzione;
                push.OnDeviceSubscriptionChanged += DeviceSubscriptionChanged;
                push.OnChannelCreated += ChannelCreated;
                push.OnChannelDestroyed += ChannelDestroyed;

                //var appleCert = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Modules\\Laser.Orchard.Mobile\\AppleCertificate\\" + setting_file));
                var appleCert = File.ReadAllBytes(setting_file);
                //        var applepassword = "laser123";
                push.RegisterAppleService(new ApplePushChannelSettings(produzione, appleCert, setting_password)); //Extension method
                //Fluent construction of an iOS notification
                //IMPORTANT: For iOS you MUST MUST MUST use your own DeviceToken here that gets generated within your iOS app itself when the Application Delegate
                //  for registered for remote notifications is called, and the device token is passed back to you

                foreach (PushNotificationVM dispositivo in listdispositivo) {
                    AppleNotification appleNotification = new AppleNotification();
                    if (!string.IsNullOrEmpty(pushMessage.Eu)) {
                        appleNotification.ForDeviceToken(dispositivo.Token)
                            .WithAlert(pushMessage.Text)
                            .WithCustomItem("Eu", pushMessage.Eu)
                            .WithSound(pushMessage.Sound);
                    }
                    else
                        if (!string.IsNullOrEmpty(pushMessage.Iu)) {
                            appleNotification.ForDeviceToken(dispositivo.Token)
                                .WithAlert(pushMessage.Text)
                                .WithCustomItem("Iu", pushMessage.Iu)
                                .WithSound(pushMessage.Sound);
                        }
                        else {
                            appleNotification.ForDeviceToken(dispositivo.Token)
                                .WithAlert(pushMessage.Text)

                                    //  .WithCustomItem("Title", pushMessage.Title)
                                .WithCustomItem("Id", pushMessage.idContent)
                                .WithCustomItem("Rid", pushMessage.idRelated)
                                .WithCustomItem("Ct", pushMessage.Ct)
                                .WithCustomItem("Al", pushMessage.Al)
                                .WithSound(pushMessage.Sound);
                        }
                    if (appleNotification.Payload.ToJson().Length > 255) {
                        _notifier.Information(T("Sent: message payload exceed the limit"));
                    }
                    else {
                        push.QueueNotification(appleNotification);

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
                push.StopAllServices();
            }
        }

        //private void PushApple(PushNotificationRecord dispositivo, PushAppleVM pushMessage) {
        //    List<PushNotificationRecord> listdispositivo = new List<PushNotificationRecord>();
        //    listdispositivo.Add(dispositivo);
        //    PushApple(listdispositivo, dispositivo.Produzione, pushMessage);
        //}

        private void DeviceSubscriptionChanged(object sender, string oldSubscriptionId, string newSubscriptionId, INotification notification) {
            //Currently this event will only ever happen for Android GCM
            _myLog.WriteLog(T("Device Registration Changed:  Old-> " + oldSubscriptionId + "  New-> " + newSubscriptionId + " -> " + notification).ToString());

            PushNotificationRecord pnr = _pushNotificationRepository.Fetch(x => x.Token == oldSubscriptionId && x.Device == TipoDispositivo.Android).FirstOrDefault();
            IEnumerable<PushNotificationRecord> esiste_il_nuovo = _pushNotificationRepository.Fetch(x => x.Token == newSubscriptionId && x.Device == TipoDispositivo.Android);
            if (esiste_il_nuovo != null && esiste_il_nuovo.FirstOrDefault() != null)
                pnr.Validated = false;
            else
                pnr.Token = newSubscriptionId;
            _pushNotificationRepository.Update(pnr);
        }

        private void NotificationSent(object sender, INotification notification) {
            if (notification is AppleNotification) {
                _myLog.WriteLog(T("Sent: " + sender + " -> " + (notification as AppleNotification).DeviceToken + " -> " + notification).ToString());
            }
            else {
            _myLog.WriteLog(T("Sent: " + sender + " -> " + notification).ToString());
            }
            messageSent++;
            //   _notifier.Information(T("Sent: " + sender + " -> " + notification));
        }

        private void NotificationFailed(object sender, INotification notification, Exception notificationFailureException) {
            _myLog.WriteLog((T("Failure: " + sender + " -> " + notificationFailureException.Message + " -> " + notification)).ToString());
        }

        private void ChannelException(object sender, IPushChannel channel, Exception exception) {
            _myLog.WriteLog(T("Channel Exception: " + sender + " -> " + exception).ToString());
        }

        private void ServiceException(object sender, Exception exception) {
            _myLog.WriteLog(T("Channel Exception: " + sender + " -> " + exception).ToString());
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

        private void ChannelDestroyed(object sender) {
            _myLog.WriteLog(T("Channel Destroyed for: " + sender).ToString());
        }

        private void ChannelCreated(object sender, IPushChannel pushChannel) {
            _myLog.WriteLog(T("Channel Created for: " + sender).ToString());
        }
    }
}