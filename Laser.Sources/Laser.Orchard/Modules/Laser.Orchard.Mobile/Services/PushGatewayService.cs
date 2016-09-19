using Laser.Orchard.CommunicationGateway.Models;
using Laser.Orchard.CommunicationGateway.Services;
using Laser.Orchard.Mobile.Models;
using Laser.Orchard.Mobile.Settings;
using Laser.Orchard.Mobile.ViewModels;
using Laser.Orchard.Queries.Models;
using Laser.Orchard.Queries.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NHibernate.Transform;
using Orchard;
using Orchard.Autoroute.Models;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.Data;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Tokens;
using Orchard.UI.Notify;
using PushSharp.Apple;
using PushSharp.Core;
using PushSharp.Google;
using PushSharp.Windows;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Hosting;
using System.Xml.Linq;

namespace Laser.Orchard.Mobile.Services {
    public interface IPushGatewayService : IDependency {
        IList GetPushQueryResult(Int32[] ids, bool countOnly = false);
        IList GetPushQueryResult(Int32[] ids, TipoDispositivo? tipodisp, bool produzione, string language, bool countOnly = false);
        void PublishedPushEventTest(ContentItem ci);
        void PublishedPushEvent(ContentItem ci);
        void SendPushService(bool produzione, string device, Int32 idContentRelated, string language_param, string messageApple, string messageAndroid, string messageWindows, string sound, string queryDevice = "", string externalUrl = "");
        IList<IDictionary> GetContactsWithDevice(string nameFilter);
        void SendPushToContact(ContentItem ci, string contactTitle);
    }

    [OrchardFeature("Laser.Orchard.PushGateway")]
    public class PushGatewayService : IPushGatewayService {
        private readonly IPushNotificationService _pushNotificationService;
        private readonly IQueryPickerService _queryPickerServices;
        private readonly IOrchardServices _orchardServices;
        private readonly ISessionLocator _sessionLocator;
        private readonly IMylogService _myLog;
        private readonly IRepository<SentRecord> _sentRepository;
        private readonly IRepository<PushNotificationRecord> _pushNotificationRepository;
        private readonly INotifier _notifier;
        private readonly ICommunicationService _communicationService;
        private readonly ITokenizer _tokenizer;
        private readonly ShellSettings _shellSetting;
        public Localizer T { get; set; }

        private Int32 messageSent;
        private object lockMonitor;
        private const int MAX_PUSH_TEXT_LENGTH = 160;

        public PushGatewayService(IPushNotificationService pushNotificationService, IQueryPickerService queryPickerServices, IOrchardServices orchardServices, ISessionLocator sessionLocator, IMylogService myLog, IRepository<SentRecord> sentRepository, IRepository<PushNotificationRecord> pushNotificationRepository, INotifier notifier, ICommunicationService communicationService, ITokenizer tokenizer, ShellSettings shellSetting) {
            _pushNotificationService = pushNotificationService;
            _queryPickerServices = queryPickerServices;
            _orchardServices = orchardServices;
            _sessionLocator = sessionLocator;
            _myLog = myLog;
            _sentRepository = sentRepository;
            _pushNotificationRepository = pushNotificationRepository;
            _notifier = notifier;
            _communicationService = communicationService;
            _tokenizer = tokenizer;
            _shellSetting = shellSetting;
            messageSent = 0;
            lockMonitor = new object();
        }

        public IList GetPushQueryResult(Int32[] ids, bool countOnly = false) {
            return GetPushQueryResult(ids, null, true, "All", countOnly);
        }

        public IList<IDictionary> GetContactsWithDevice(string nameFilter) {
            string query = "SELECT tp.Title as Title, count(MobileRecord.Id) as NumDevice" +
                " FROM Orchard.ContentManagement.Records.ContentItemVersionRecord as civr " +
                " join civr.ContentItemRecord as cir " +
                " join cir.CommunicationContactPartRecord as CommunicationContact " +
                " join cir.MobileContactPartRecord as MobileContact " +
                " join civr.TitlePartRecord as tp" +
                " join MobileContact.MobileRecord as MobileRecord " +
                " WHERE civr.Published=1 AND MobileRecord.Validated"+
                " AND tp.Title like '%" + nameFilter.Replace("'", "''") + "%'";
            string hostCheck = _shellSetting.RequestUrlHost ?? "";
            string prefixCheck = _shellSetting.RequestUrlPrefix ?? "";
            string machineNameCheck = System.Environment.MachineName ?? "";
            query += string.Format(" AND MobileRecord.RegistrationUrlHost='{0}' AND MobileRecord.RegistrationUrlPrefix='{1}' AND MobileRecord.RegistrationMachineName='{2}'", hostCheck.Replace("'", "''"), prefixCheck.Replace("'", "''"), machineNameCheck.Replace("'", "''"));
            query += " GROUP BY tp.Title";
            var fullStatement = _sessionLocator.For(null)
                .CreateQuery(query)
                .SetCacheable(false);
            var lista = fullStatement
                .SetResultTransformer(Transformers.AliasToEntityMap)
                 .List<IDictionary>();
            return lista;
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
                queryForPush = "SELECT MobileRecord.Id as Id, MobileRecord.Device as Device, MobileRecord.Produzione as Produzione, MobileRecord.Validated as Validated, MobileRecord.Language as Language, MobileRecord.UUIdentifier as UUIdentifier, MobileRecord.Token as Token, MobileRecord.RegistrationUrlHost as RegistrationUrlHost, MobileRecord.RegistrationUrlPrefix as RegistrationUrlPrefix, MobileRecord.RegistrationMachineName as RegistrationMachineName";
            }
            queryForPush += " FROM Orchard.ContentManagement.Records.ContentItemVersionRecord as civr " +
                "join civr.ContentItemRecord as cir " +
                "join cir.CommunicationContactPartRecord as CommunicationContact " +
                "join cir.MobileContactPartRecord as MobileContact " +
                "join MobileContact.MobileRecord as MobileRecord " +
                "WHERE civr.Published=1 AND MobileRecord.Validated";
            string hostCheck = _shellSetting.RequestUrlHost ?? "";
            string prefixCheck = _shellSetting.RequestUrlPrefix ?? "";
            string machineNameCheck = System.Environment.MachineName ?? "";
            queryForPush += string.Format(" AND MobileRecord.RegistrationUrlHost='{0}' AND MobileRecord.RegistrationUrlPrefix='{1}' AND MobileRecord.RegistrationMachineName='{2}'", hostCheck.Replace("'", "''"), prefixCheck.Replace("'", "''"), machineNameCheck.Replace("'", "''"));
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

        private List<PushNotificationVM> GetDevicesByContact(string contactTitle) {
            var lista = new List<PushNotificationVM>();
            string query = "SELECT MobileRecord.Id as Id, MobileRecord.Device as Device, MobileRecord.Produzione as Produzione, MobileRecord.Validated as Validated, MobileRecord.Language as Language, MobileRecord.UUIdentifier as UUIdentifier, MobileRecord.Token as Token, MobileRecord.RegistrationUrlHost as RegistrationUrlHost, MobileRecord.RegistrationUrlPrefix as RegistrationUrlPrefix, MobileRecord.RegistrationMachineName as RegistrationMachineName" +
                " FROM Orchard.ContentManagement.Records.ContentItemVersionRecord as civr " +
                " join civr.ContentItemRecord as cir " +
                " join cir.CommunicationContactPartRecord as CommunicationContact " +
                " join cir.MobileContactPartRecord as MobileContact " +
                " join civr.TitlePartRecord as tp" +
                " join MobileContact.MobileRecord as MobileRecord " +
                " WHERE civr.Published=1 AND MobileRecord.Validated" +
                " AND tp.Title='" + contactTitle.Replace("'", "''") + "'";
            string hostCheck = _shellSetting.RequestUrlHost ?? "";
            string prefixCheck = _shellSetting.RequestUrlPrefix ?? "";
            string machineNameCheck = System.Environment.MachineName ?? "";
            query += string.Format(" AND MobileRecord.RegistrationUrlHost='{0}' AND MobileRecord.RegistrationUrlPrefix='{1}' AND MobileRecord.RegistrationMachineName='{2}'", hostCheck.Replace("'", "''"), prefixCheck.Replace("'", "''"), machineNameCheck.Replace("'", "''"));
            var fullStatement = _sessionLocator.For(null)
                .CreateQuery(query)
                .SetCacheable(false);
            var elenco = fullStatement
                .SetResultTransformer(Transformers.AliasToEntityMap)
                 .List<IDictionary>();
            foreach (Hashtable ht in elenco) {
                lista.Add(new PushNotificationVM {
                    Id = Convert.ToInt32(ht["Id"]),
                    Device = (TipoDispositivo)(Enum.Parse(typeof(TipoDispositivo), Convert.ToString(ht["Device"]))),
                    Produzione = Convert.ToBoolean(ht["Produzione"], CultureInfo.InvariantCulture),
                    Validated = Convert.ToBoolean(ht["Validated"], CultureInfo.InvariantCulture),
                    Language = Convert.ToString(ht["Language"]),
                    UUIdentifier = Convert.ToString(ht["UUIdentifier"]),
                    Token = Convert.ToString(ht["Token"]),
                    RegistrationUrlHost = Convert.ToString(ht["RegistrationUrlHost"]),
                    RegistrationUrlPrefix = Convert.ToString(ht["RegistrationUrlPrefix"]),
                    RegistrationMachineName = Convert.ToString(ht["RegistrationMachineName"])
                });
            }
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

        public void SendPushToContact(ContentItem ci, string contactTitle) {
            // ricava l'elenco dei device
            var elencoDevice = GetDevicesByContact(contactTitle);

            MobilePushPart mpp = ci.As<MobilePushPart>();
            // ricava l'id del related content
            int idContentRelated = 0;
            var relatedContent = ((dynamic)ci).MobilePushPart.RelatedContent;
            // nel caso in cui la MobilePushPart sia contenuta nel content type CommunicationAdvertising, usa il related content di quest'ultimo
            if (ci.ContentType == "CommunicationAdvertising") {
                relatedContent = ((dynamic)ci).CommunicationAdvertisingPart.ContentLinked;
            }
            if (relatedContent != null && relatedContent.Ids != null && ((int[])relatedContent.Ids).Count() > 0) {
                idContentRelated = relatedContent.Ids[0];
            }

            // compone il messaggio da inviare
            PushMessage pushMessage = GeneratePushMessage(mpp, ci.Id, idContentRelated);

            // invia la push a ogni device
            List<PushNotificationVM> singoloDevice = null;
            foreach (var device in elencoDevice) {
                singoloDevice = new List<PushNotificationVM>();
                singoloDevice.Add(device);
                if(device.Device == TipoDispositivo.Android) {
                    PushAndroid(singoloDevice, device.Produzione, pushMessage, true);
                }
                if (device.Device == TipoDispositivo.Apple) {
                    PushApple(singoloDevice, device.Produzione, pushMessage, true);
                }
                if (device.Device == TipoDispositivo.WindowsMobile) {
                    PushWindows(singoloDevice, device.Produzione, pushMessage, true);
                }
            }
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
        public void SendPushService(bool produzione, string device, Int32 idContentRelated, string language_param, string messageApple, string messageAndroid, string messageWindows, string sound, string queryDevice = "", string externalUrl = "") {
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
                else {
                    language = language_param;
                }
                _myLog.WriteLog("language:" + language);
                _myLog.WriteLog("Send to:" + device);
                if (device == "All") {
                    PushMessage pushandroid = new PushMessage();
                    pushandroid.idContent = 0;
                    pushandroid.idRelated = idContentRelated;
                    pushandroid.Text = messageAndroid;
                    pushandroid.Ct = ctype;
                    pushandroid.Al = displayalias;
                    pushandroid.Eu = externalUrl;
                    SendAllAndroid("unknown", pushandroid, produzione, language, queryDevice);

                    PushMessage pushapple = new PushMessage();
                    pushapple.idContent = 0;
                    pushapple.idRelated = idContentRelated;
                    pushapple.Sound = sound;
                    pushapple.Text = messageApple;
                    pushapple.Title = "";
                    pushapple.Ct = ctype;
                    pushapple.Al = displayalias;
                    pushapple.Eu = externalUrl;
                    pushapple.ValidPayload = true;
                    SendAllApple("unknown", pushapple, produzione, language, queryDevice);
                    
                    PushMessage pushwindows = new PushMessage();
                    pushwindows.idContent = 0;
                    pushwindows.idRelated = idContentRelated;
                    pushwindows.Text = messageAndroid;
                    pushwindows.Ct = ctype;
                    pushwindows.Al = displayalias;
                    pushwindows.Eu = externalUrl;
                    SendAllWindows(ctype, pushwindows, produzione, language, queryDevice);
                }
                if (device == TipoDispositivo.Android.ToString()) {
                    PushMessage pushandroid = new PushMessage();
                    pushandroid.idContent = 0;
                    pushandroid.idRelated = idContentRelated;
                    pushandroid.Text = messageAndroid;
                    pushandroid.Ct = ctype;
                    pushandroid.Al = displayalias;
                    pushandroid.Eu = externalUrl;
                    SendAllAndroid("unknown", pushandroid, produzione, language, queryDevice);
                }
                if (device == TipoDispositivo.Apple.ToString()) {
                    PushMessage pushapple = new PushMessage();
                    pushapple.idContent = 0;
                    pushapple.idRelated = idContentRelated;
                    pushapple.Sound = sound;
                    pushapple.Text = messageApple;
                    pushapple.Title = "";
                    pushapple.Ct = ctype;
                    pushapple.Al = displayalias;
                    pushapple.Eu = externalUrl;
                    pushapple.ValidPayload = true;
                    SendAllApple("unknown", pushapple, produzione, language, queryDevice);
                }
                if (device == TipoDispositivo.WindowsMobile.ToString()) {
                    PushMessage pushwindows = new PushMessage();
                    pushwindows.idContent = 0;
                    pushwindows.idRelated = idContentRelated;
                    pushwindows.Text = messageAndroid;
                    pushwindows.Ct = ctype;
                    pushwindows.Al = displayalias;
                    pushwindows.Eu = externalUrl;
                    SendAllWindows(ctype, pushwindows, produzione, language, queryDevice);
                }
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
            catch {
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

            // it is a test so all submissions are repeatable
            if (locTipoDispositivo.HasValue == false) {// tutti
                SendAllAndroidPart(mpp, idContent, idContentRelated, language, produzione, queryDevice, ids, repeatable:true);
                SendAllApplePart(mpp, idContent, idContentRelated, language, produzione, queryDevice, ids, repeatable: true);
                SendAllWindowsPart(mpp, idContent, idContentRelated, language, produzione, queryDevice, ids, repeatable: true);
            }
            else if (locTipoDispositivo.Value == TipoDispositivo.Android) {
                SendAllAndroidPart(mpp, idContent, idContentRelated, language, produzione, queryDevice, ids, repeatable: true);
            }
            else if (locTipoDispositivo.Value == TipoDispositivo.Apple) {
                SendAllApplePart(mpp, idContent, idContentRelated, language, produzione, queryDevice, ids, repeatable: true);
            }
            else if (locTipoDispositivo.Value == TipoDispositivo.WindowsMobile) {
                SendAllWindowsPart(mpp, idContent, idContentRelated, language, produzione, queryDevice, ids, repeatable: true);
            }
        }

        public void PublishedPushEvent(ContentItem ci) {
            try {
                _myLog.WriteLog("Iniziato invio Push del content " + ci.Id);
                ContentItem savedCi = _orchardServices.ContentManager.Get(ci.Id);
                MobilePushPart mpp = ci.As<MobilePushPart>();
                if (mpp.ToPush) {
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

                        if (locTipoDispositivo.HasValue == false) {// tutti
                            SendAllAndroidPart(mpp, idContent, idContentRelated, language, produzione, queryDevice, ids);
                            SendAllApplePart(mpp, idContent, idContentRelated, language, produzione, queryDevice, ids);
                            SendAllWindowsPart(mpp, idContent, idContentRelated, language, produzione, queryDevice, ids);
                        }
                        else if (locTipoDispositivo.Value == TipoDispositivo.Android) {
                            SendAllAndroidPart(mpp, idContent, idContentRelated, language, produzione, queryDevice, ids);
                        }
                        else if (locTipoDispositivo.Value == TipoDispositivo.Apple) {
                            SendAllApplePart(mpp, idContent, idContentRelated, language, produzione, queryDevice, ids);
                        }
                        else if (locTipoDispositivo.Value == TipoDispositivo.WindowsMobile) {
                            SendAllWindowsPart(mpp, idContent, idContentRelated, language, produzione, queryDevice, ids);
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
                catch { 
                    // ignora volutamente qualsiasi errore
                }
                _myLog.WriteLog("Terminato invio Push del content " + ci.Id + " " + title);
            }
            catch (Exception ex) {
                string title = "no title";
                try {
                    title = ci.As<TitlePart>().Title;
                }
                catch {
                    // ignora volutamente qualsiasi errore
                }
                _myLog.WriteLog(ex.Message);
                _myLog.WriteLog("Errore invio Push del content " + ci.Id + " " + title);
            }
        }

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

        private PushMessage GeneratePushMessage(MobilePushPart mpp, Int32 idcontent, Int32 idContentRelated) {
            PushMessage pushMessage = new PushMessage();
            pushMessage.Text = mpp.TextPush;
            pushMessage.Title = mpp.TitlePush;
            pushMessage.ValidPayload = true;
            if (mpp.ContentItem.ContentType == "CommunicationAdvertising") {
                if (idContentRelated > 0) {
                    pushMessage.Iu = idContentRelated.ToString();
                }
                else if (!string.IsNullOrEmpty(((dynamic)(mpp.ContentItem.As<CommunicationAdvertisingPart>())).UrlLinked.Value)) {
                    string shortlink = _communicationService.GetCampaignLink("Push", mpp);
                    pushMessage.Eu = shortlink;
                }
            }
            else {
                string ctype = "";
                string displayalias = "";
                var extra = getextrainfo(idContentRelated > 0 ? idContentRelated : idcontent);
                ctype = extra[0];
                displayalias = extra[1];
                pushMessage.idContent = idcontent;
                pushMessage.idRelated = idContentRelated;
                pushMessage.Ct = ctype;
                pushMessage.Al = displayalias;
                var partSettings = mpp.Settings.GetModel<PushMobilePartSettingVM>();
                if (!(partSettings.AcceptZeroRelated) && pushMessage.idRelated == 0) {
                    pushMessage.idRelated = pushMessage.idContent;
                }
            }
            return pushMessage;
        }

        private void SendAllAndroidPart(MobilePushPart mpp, Int32 idcontent, Int32 idContentRelated, string language, bool produzione, string queryDevice, int[] queryIds, bool repeatable=false) {
            PushMessage pushMessage = GeneratePushMessage(mpp, idcontent, idContentRelated);
            SendAllAndroid(mpp.ContentItem.ContentType, pushMessage, produzione, language, queryDevice, queryIds, repeatable);
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
                            Device = (TipoDispositivo)(Enum.Parse(typeof(TipoDispositivo), Convert.ToString(ht["Device"]))),
                            Produzione = Convert.ToBoolean(ht["Produzione"], CultureInfo.InvariantCulture),
                            Validated = Convert.ToBoolean(ht["Validated"], CultureInfo.InvariantCulture),
                            Language = Convert.ToString(ht["Language"]),
                            UUIdentifier = Convert.ToString(ht["UUIdentifier"]),
                            Token = Convert.ToString(ht["Token"]),
                            RegistrationUrlHost = Convert.ToString(ht["RegistrationUrlHost"]),
                            RegistrationUrlPrefix = Convert.ToString(ht["RegistrationUrlPrefix"]),
                            RegistrationMachineName = Convert.ToString(ht["RegistrationMachineName"])
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
                                Token = pnr.Token,
                                RegistrationUrlHost = pnr.RegistrationUrlHost,
                                RegistrationUrlPrefix = pnr.RegistrationUrlPrefix,
                                RegistrationMachineName = pnr.RegistrationMachineName
                            });
                        }
                    }
                    else {
                        var estrazione = _sessionLocator.For(typeof(PushNotificationRecord))
                            .CreateSQLQuery(string.Format("select Id, Device, Produzione, Validated, Language, UUIdentifier, Token, RegistrationUrlHost, RegistrationUrlPrefix, RegistrationMachineName from ( {0} ) x where x.Device = '{1}' and x.Produzione = {2} and x.Validated = 1 and (x.Language = '{3}' or '{3}' = 'All') ", queryDevice, tipodisp, (produzione) ? 1 : 0, language))
                         .List();
                        object[] ht = null;
                        foreach (var arr in estrazione) {
                            ht = (object[])arr;
                            lista.Add(new PushNotificationVM {
                                Id = Convert.ToInt32(ht[0]),
                                Device = (TipoDispositivo)(Enum.Parse(typeof(TipoDispositivo), Convert.ToString(ht[1]))),
                                Produzione = Convert.ToBoolean(ht[2], CultureInfo.InvariantCulture),
                                Validated = Convert.ToBoolean(ht[3], CultureInfo.InvariantCulture),
                                Language = Convert.ToString(ht[4]),
                                UUIdentifier = Convert.ToString(ht[5]),
                                Token = Convert.ToString(ht[6]),
                                RegistrationUrlHost = Convert.ToString(ht[7]),
                                RegistrationUrlPrefix = Convert.ToString(ht[8]),
                                RegistrationMachineName = Convert.ToString(ht[9])
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

        private void SendAllAndroid(string contenttype, PushMessage pushMessage, bool produzione, string language, string queryDevice = "", int[] queryIds = null, bool repeatable=false) {
            var allDevice = GetListMobileDevice(contenttype, queryDevice, TipoDispositivo.Android, produzione, language, queryIds);
            PushAndroid(allDevice, produzione, pushMessage, repeatable);
        }

        private void SendAllApplePart(MobilePushPart mpp, Int32 idcontent, Int32 idContentRelated, string language, bool produzione, string queryDevice, int[] queryIds, bool repeatable = false) {
            PushMessage newpush = new PushMessage();
            newpush = GeneratePushMessage(mpp, idcontent, idContentRelated);
            if (newpush.Text.Length > MAX_PUSH_TEXT_LENGTH) {
                _notifier.Information(T("Apple send: message payload exceed the limit"));
                _myLog.WriteLog("Apple send: message payload exceed the limit");
                newpush.ValidPayload = false;
            }
            SendAllApple(mpp.ContentItem.ContentType, newpush, produzione, language, queryDevice, queryIds, repeatable);
        }

        private void SendAllApple(string contenttype, PushMessage newpush, bool produzione, string language, string queryDevice = "", int[] queryIds = null, bool repeatable = false) {
            var allDevice = GetListMobileDevice(contenttype, queryDevice, TipoDispositivo.Apple, produzione, language, queryIds);
            if (newpush.ValidPayload) {
                PushApple(allDevice, produzione, newpush, repeatable);
            }
        }

        private void SendAllWindowsPart(MobilePushPart mpp, Int32 idcontent, Int32 idContentRelated, string language, bool produzione, string queryDevice, int[] queryIds, bool repeatable = false) {
            PushMessage pushMessage = new PushMessage {
                Text = mpp.TextPush,
                Title = mpp.TitlePush,
                idContent = idcontent,
                idRelated = idContentRelated
            };
            SendAllWindows(mpp.ContentItem.ContentType, pushMessage, produzione, language, queryDevice, queryIds, repeatable);
        }
        private void SendAllWindows(string contenttype, PushMessage pushMessage, bool produzione, string language, string queryDevice = "", int[] queryIds = null, bool repeatable = false) {
            var allDevice = GetListMobileDevice(contenttype, queryDevice, TipoDispositivo.WindowsMobile, produzione, language, queryIds);
            PushWindows(allDevice, produzione, pushMessage, repeatable);
        }
        private List<PushNotificationVM> RemoveSent(List<PushNotificationVM> listdispositivo, Int32 IdContent) {
            if (IdContent > 0) {
                List<Int32> listainvii = _sentRepository.Fetch(x => x.PushedItem == IdContent).Select(y => y.PushNotificationRecord_Id).ToList();
                return listdispositivo.Where(x => !listainvii.Contains(x.Id)).ToList();
            }
            else {
                return listdispositivo;
            }
        }
        private void PushAndroid(List<PushNotificationVM> listdispositivo, bool produzione, PushMessage pushMessage, bool repeatable = false) {
            if (repeatable == false) {
                listdispositivo = RemoveSent(listdispositivo, pushMessage.idContent);
            }
            string setting = "";
            if (produzione)
                setting = _orchardServices.WorkContext.CurrentSite.As<PushMobileSettingsPart>().AndroidApiKey;
            else
                setting = _orchardServices.WorkContext.CurrentSite.As<PushMobileSettingsPart>().AndroidApiKeyDevelopment;
            var config = new GcmConfiguration(setting);
            var serviceUrl = _orchardServices.WorkContext.CurrentSite.As<PushMobileSettingsPart>().AndroidPushServiceUrl;
            if (string.IsNullOrWhiteSpace(serviceUrl)) {
                // default: FCM
                config.OverrideUrl("https://fcm.googleapis.com/fcm/send");
            }
            else {
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
            // compone il payload
            StringBuilder sb = new StringBuilder();
            sb.Clear();
            sb.AppendFormat("{{ \"Text\": \"{0}\"", FormatJsonValue(pushMessage.Text));
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
            // sezione notification
            StringBuilder sbNotification = new StringBuilder();
            sbNotification.Clear();
            sbNotification.AppendFormat("{{ \"body\": \"{0}\"", FormatJsonValue(pushMessage.Text));
            //sbNotification.AppendFormat(",\"title\":\"{0}\"", FormatJsonValue(pushMessage.Text));
            //sbNotification.AppendFormat(",\"icon\":\"{0}\"", "new");
            sbNotification.Append("}");

            string hostCheck = _shellSetting.RequestUrlHost ?? "";
            string prefixCheck = _shellSetting.RequestUrlPrefix ?? "";
            string machineNameCheck = System.Environment.MachineName ?? "";
            push.Start();
            foreach (PushNotificationVM pnr in listdispositivo) {
                // verifica che il device sia stato registrato nell'ambiente corrente
                if ((pnr.RegistrationUrlHost == hostCheck) && (pnr.RegistrationUrlPrefix == prefixCheck) && (pnr.RegistrationMachineName == machineNameCheck)) {
                    push.QueueNotification(new GcmNotification {
                        RegistrationIds = new List<string> { pnr.Token },
                        Notification = JObject.Parse(sbNotification.ToString()),
                        Data = JObject.Parse(sb.ToString()),
                        Priority = GcmNotificationPriority.High, // necessario per bypassare il fatto che l'app non sia in whitelist
                        TimeToLive = 172800 //2 giorni espressi in secondi
                    });

                    if ((repeatable == false) && (pushMessage.idContent > 0)) {
                        SentRecord sr = new SentRecord();
                        sr.DeviceType = "Android";
                        sr.PushNotificationRecord_Id = pnr.Id;
                        sr.PushedItem = pushMessage.idContent;
                        sr.SentDate = DateTime.UtcNow;
                        _sentRepository.Create(sr);
                        _sentRepository.Flush();
                    }
                }
            }
            push.Stop();
        }
        private void PushApple(List<PushNotificationVM> listdispositivo, bool produzione, PushMessage pushMessage, bool repeatable = false) {
            if (repeatable == false) {
                listdispositivo = RemoveSent(listdispositivo, pushMessage.idContent);
            }
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
                // compone il payload
                if (pushMessage.Text.Length > MAX_PUSH_TEXT_LENGTH) {
                    _notifier.Information(T("Sent: message payload exceed the limit"));
                    _myLog.WriteLog("Sent: message payload exceed the limit");
                }
                else {
                    StringBuilder sb = new StringBuilder();
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

                    string hostCheck = _shellSetting.RequestUrlHost ?? "";
                    string prefixCheck = _shellSetting.RequestUrlPrefix ?? "";
                    string machineNameCheck = System.Environment.MachineName ?? "";
                    push.Start();
                    foreach (PushNotificationVM dispositivo in listdispositivo) {
                        // verifica che il device sia stato registrato nell'ambiente corrente
                        if ((dispositivo.RegistrationUrlHost == hostCheck) && (dispositivo.RegistrationUrlPrefix == prefixCheck) && (dispositivo.RegistrationMachineName == machineNameCheck)) {
                            push.QueueNotification(new ApnsNotification {
                                DeviceToken = dispositivo.Token,
                                Payload = JObject.Parse(sb.ToString())
                            });

                            if ((repeatable == false) && (pushMessage.idContent > 0)) {
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
        }
        private void PushWindows(List<PushNotificationVM> listdispositivo, bool produzione, PushMessage pushMessage, bool repeatable = false) {
            if (repeatable == false) {
                listdispositivo = RemoveSent(listdispositivo, pushMessage.idContent);
            }

            // TODO: gestire settaggi produzione/sviluppo?
            var setting_WindowsAppPackageName = _orchardServices.WorkContext.CurrentSite.As<PushMobileSettingsPart>().WindowsAppPackageName;
            var setting_WindowsAppSecurityIdentifier = _orchardServices.WorkContext.CurrentSite.As<PushMobileSettingsPart>().WindowsAppSecurityIdentifier;
            var setting_WindowsEndPoint = _orchardServices.WorkContext.CurrentSite.As<PushMobileSettingsPart>().WindowsEndPoint;
            var config = new WnsConfiguration(setting_WindowsAppPackageName, setting_WindowsAppSecurityIdentifier, setting_WindowsEndPoint);
            var push = new WnsServiceBroker(config);
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

            // genera il payload
            string message = string.Format(@"
            <toast>
                <visual>
                    <binding template=""ToastGeneric"">
                        <text>{0}</text>
                    </binding>  
                </visual>
            </toast>", pushMessage.Text);

            string hostCheck = _shellSetting.RequestUrlHost ?? "";
            string prefixCheck = _shellSetting.RequestUrlPrefix ?? "";
            string machineNameCheck = System.Environment.MachineName ?? "";
            push.Start();
            foreach (PushNotificationVM pnr in listdispositivo) {
                // verifica che il device sia stato registrato nell'ambiente corrente
                if ((pnr.RegistrationUrlHost == hostCheck) && (pnr.RegistrationUrlPrefix == prefixCheck) && (pnr.RegistrationMachineName == machineNameCheck)) {
                    push.QueueNotification(new WnsToastNotification {
                        ChannelUri = pnr.Token,
                        Payload = XElement.Parse(message)
                    });

                    if ((repeatable == false) && (pushMessage.idContent > 0)) {
                        SentRecord sr = new SentRecord();
                        sr.DeviceType = "Windows";
                        sr.PushNotificationRecord_Id = pnr.Id;
                        sr.PushedItem = pushMessage.idContent;
                        sr.SentDate = DateTime.UtcNow;
                        _sentRepository.Create(sr);
                        _sentRepository.Flush();
                    }
                }
            }
            push.Stop();
        }
        private string FormatJsonValue(string text) {
            return (text ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"");
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
            lock (lockMonitor) {
                PushNotificationRecord pnr = _pushNotificationRepository.Fetch(x => x.Token == oldSubscriptionId && x.Device == tipoDispositivo).FirstOrDefault();
                IEnumerable<PushNotificationRecord> esiste_il_nuovo = _pushNotificationRepository.Fetch(x => x.Token == newSubscriptionId && x.Device == tipoDispositivo);
                if (esiste_il_nuovo != null && esiste_il_nuovo.FirstOrDefault() != null)
                    pnr.Validated = false;
                else
                    pnr.Token = newSubscriptionId;
                _pushNotificationRepository.Update(pnr);
                _pushNotificationRepository.Flush();
            }
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
            lock (lockMonitor) {
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
        }

        private void DeviceSubscriptionExpiredWindowsMobile(object sender, string expiredDeviceSubscriptionId, DateTime timestamp, INotification notification) {
            _myLog.WriteLog(T("Device Subscription Expired: " + sender + " -> " + expiredDeviceSubscriptionId).ToString());
            // ToDo
            _myLog.WriteLog(T("The event is not implemented for Windows").ToString());
        }
}
}