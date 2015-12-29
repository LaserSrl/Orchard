using Laser.Orchard.CommunicationGateway.Models;
using Laser.Orchard.Mobile.Models;
using Laser.Orchard.Mobile.Settings;
using Laser.Orchard.Mobile.ViewModels;
using Newtonsoft.Json;
using Orchard;
using Orchard.Autoroute.Models;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.Data;
using Orchard.Environment.Configuration;
using Orchard.Localization;
using Orchard.Tokens;
using Orchard.UI.Notify;
using PushSharp;
using PushSharp.Android;
using PushSharp.Apple;
using PushSharp.Core;
using PushSharp.Windows;
using PushSharp.WindowsPhone;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;

namespace Laser.Orchard.Mobile.Services {

    public interface IPushNotificationService : IDependency {

        void StorePushNotification(PushNotificationRecord pushElement);

        IEnumerable<PushNotificationRecord> SearchPushNotification(string texttosearch);

        //  void SendPush(Int32 iddispositivo, string message);
        void PublishedPushEvent(dynamic mycontext, ContentItem ci);

        void SendPushService(bool produzione, string device, Int32 idContentRelated, string language_param, string messageApple, string messageAndroid, string JsonAndroid, string messageWindows, string sound, string queryDevice = "");

        void Synchronize();
    }

    public class PushNotificationService : IPushNotificationService {
        private readonly IRepository<PushNotificationRecord> _pushNotificationRepository;
        private readonly IRepository<UserDeviceRecord> _userDeviceRecord;
        public Localizer T { get; set; }
        private readonly INotifier _notifier;
        private readonly IOrchardServices _orchardServices;
        private readonly IMylogService _myLog;
        private readonly ShellSettings _shellSetting;
        private readonly ISessionLocator _sessionLocator;

        //private readonly ICommunicationService _communicationService;
        private readonly ITokenizer _tokenizer;

        private Int32 messageAppleSent;
        private string QueryDevice;

        public PushNotificationService(
                IOrchardServices orchardServices,
                IRepository<PushNotificationRecord> pushNotificationRepository,
                IRepository<UserDeviceRecord> userDeviceRecord,
                INotifier notifier,
                IMylogService myLog,
                ShellSettings shellSetting,
                ISessionLocator sessionLocator,
                ITokenizer tokenizer
            //   ,ICommunicationService communicationService
         ) {
            _orchardServices = orchardServices;
            T = NullLocalizer.Instance;
            _pushNotificationRepository = pushNotificationRepository;
            _notifier = notifier;
            _myLog = myLog;
            messageAppleSent = 0;
            _shellSetting = shellSetting;
            _sessionLocator = sessionLocator;
            QueryDevice = "";
            _tokenizer = tokenizer;
            _userDeviceRecord = userDeviceRecord;
            //  _communicationService = communicationService;
        }

        public void Synchronize() {
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
                        rec.CommunicationContactPartRecord_Id = idci;
                    }
                    _pushNotificationRepository.Flush();
                }
            }
            _notifier.Add(NotifyType.Information, T("Linked {0} user's mobile device", lUdr.Count().ToString()));

            #region [lego i rimanenti content al Content Master per renderli querabili]

            if (_orchardServices.ContentManager.Query<CommunicationContactPart, CommunicationContactPartRecord>().Where(y => y.Master).Count() == 0) {
                var Contact = _orchardServices.ContentManager.New("CommunicationContact");
                _orchardServices.ContentManager.Create(Contact);
                Contact.As<TitlePart>().Title = "Master Content";
                Contact.As<CommunicationContactPart>().Master = true;
            }
            CommunicationContactPart master = _orchardServices.ContentManager.Query<CommunicationContactPart, CommunicationContactPartRecord>().Where(y => y.Master).List().FirstOrDefault();
            int idmaster = master.Id;
            var notificationrecords = _pushNotificationRepository.Fetch(x => x.CommunicationContactPartRecord_Id == 0).ToList();
            foreach (PushNotificationRecord rec in notificationrecords) {
                rec.CommunicationContactPartRecord_Id = idmaster;
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
                _pushNotificationRepository.Update(pushElement);
            }
            else {
                pushElement.Id = 0;
                pushElement.DataInserimento = adesso;
                pushElement.DataModifica = adesso;
                _pushNotificationRepository.Create(pushElement);
            }
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
                    catch (Exception ex) {
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
                        SendAllAndroid(pushandroid, produzione, language, queryDevice);
                    }
                    else {
                        SendAllAndroidJson(JsonAndroid, produzione, language, queryDevice);
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
                    SendAllApple(pushapple, produzione, language, queryDevice);
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
                        SendAllAndroid(pushandroid, produzione, language, queryDevice);
                    }
                    else {
                        SendAllAndroidJson(JsonAndroid, produzione, language, queryDevice);
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
                    SendAllApple(pushapple, produzione, language, queryDevice);
                }
                //TODO: windows
                //if (device == TipoDispositivo.WindowsMobile.ToString()) {
                //    SendAllWindowsMobile(ci.As<MobilePushPart>(), idContent, idContentRelated, language);
                //}
            }
        }

        public void PublishedPushEvent(dynamic mycontext, ContentItem ci) {
            if (ci.As<MobilePushPart>().ToPush) {
                bool stopPush = false;
                Int32 idContent = ci.As<MobilePushPart>().Id;
                var relatedContent = ((dynamic)ci).MobilePushPart.RelatedContent;
                ContentItem relatedContentItem = null;
                Int32 idContentRelated = 0;
                dynamic contentForPush;
                dynamic ciDynamic = (dynamic)ci;
                if (relatedContent != null && relatedContent.Ids != null && ((int[])relatedContent.Ids).Count() > 0) {
                    contentForPush = (dynamic)relatedContentItem;
                    idContentRelated = relatedContent.Ids[0];
                    relatedContentItem = _orchardServices.ContentManager.Get(idContentRelated);
                    if (!relatedContentItem.IsPublished()) {
                        _notifier.Information(T("No push will be sent, related content must be published"));
                        stopPush = true;
                    }
                }
                else
                    contentForPush = ciDynamic;
                if (!stopPush) {
                    ci.As<MobilePushPart>().ToPush = false;
                    string language = _orchardServices.WorkContext.CurrentSite.SiteCulture;
                    try {
                        language = contentForPush.LocalizationPart.Culture != null ? contentForPush.LocalizationPart.Culture.Culture : language;
                    }
                    catch (Exception ex) {
                        language = "All";
                    }
                    _myLog.WriteLog("language:" + language);
                    _myLog.WriteLog("Send to:" + ci.As<MobilePushPart>().DevicePush);

                    var Myobject = new Dictionary<string, object> { { "Content", mycontext.ContentItem } };
                    string queryDevice = GetQueryDevice(Myobject, ci.As<MobilePushPart>());

                    if (ci.As<MobilePushPart>().DevicePush == "All") {
                        SendAllAndroidPart(ci.As<MobilePushPart>(), idContent, idContentRelated, language, queryDevice);

                        SendAllApplePart(ci.As<MobilePushPart>(), idContent, idContentRelated, language, queryDevice);

                        SendAllWindowsMobilePart(ci.As<MobilePushPart>(), idContent, idContentRelated, language, queryDevice);
                    }
                    if (ci.As<MobilePushPart>().DevicePush == TipoDispositivo.Android.ToString()) {
                        SendAllAndroidPart(ci.As<MobilePushPart>(), idContent, idContentRelated, language, queryDevice);
                    }
                    if (ci.As<MobilePushPart>().DevicePush == TipoDispositivo.Apple.ToString()) {
                        SendAllApplePart(ci.As<MobilePushPart>(), idContent, idContentRelated, language, queryDevice);
                    }
                    if (ci.As<MobilePushPart>().DevicePush == TipoDispositivo.WindowsMobile.ToString()) {
                        SendAllWindowsMobilePart(ci.As<MobilePushPart>(), idContent, idContentRelated, language, queryDevice);
                    }
                    _notifier.Information(T("Notification sent: " + messageAppleSent.ToString()));
                }
            }
            //      }
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
            catch (Exception ex) { }

            return extrainfo;
        }

        private void SendAllAndroidPart(MobilePushPart mpp, Int32 idcontent, Int32 idContentRelated, string language, string queryDevice) {
            PushAndroidVM newpush = new PushAndroidVM();
            string ctype = "";
            string displayalias = "";
            var extra = getextrainfo(idContentRelated > 0 ? idContentRelated : idcontent);
            ctype = extra[0];
            displayalias = extra[1];

            newpush = GenerateAndroidPush(mpp, idcontent, idContentRelated, ctype, displayalias);
            bool produzione = true;
            if (_orchardServices.WorkContext.CurrentSite.As<PushMobileSettingsPart>().ShowTestOptions)
                produzione = !(mpp.TestPush);
            SendAllAndroid(newpush, produzione, language, queryDevice);
        }

        private void SendAllAndroid(PushAndroidVM newpush, bool produzione, string language, string queryDevice = "") {
            string message = JsonConvert.SerializeObject(newpush);
            SendAllAndroidJson(message, produzione, language, queryDevice);
        }

        private string GetQueryDevice(Dictionary<string, object> contesto, MobilePushPart mpp) {
            string withtoken = mpp.Settings.GetModel<PushMobilePartSettingVM>().QueryDevice;
            if (string.IsNullOrEmpty(withtoken))
                return "";
            else
                return _tokenizer.Replace(withtoken.Replace("\r\n", " ").Replace("\t", " "), contesto);
        }

        private IEnumerable<PushNotificationRecord> GetListMobileDevice(string queryDevice, TipoDispositivo tipodisp, bool produzione, string language) {
            if (queryDevice.Trim() == "")
                return _pushNotificationRepository.Fetch(x => x.Device == tipodisp && x.Produzione == produzione && x.Validated == true && (x.Language == language || language == "All"));
            else {
                var estrazione = _sessionLocator.For(typeof(PushNotificationRecord))
                 .CreateSQLQuery(queryDevice)
                 .AddEntity(typeof(PushNotificationRecord))
                 .List<PushNotificationRecord>();
                return estrazione.Where(x => x.Device == tipodisp && x.Produzione == produzione && x.Validated == true && (x.Language == language || language == "All"));
            }
        }

        private void SendAllAndroidJson(string JsonMessage, bool produzione, string language, string queryDevice = "") {
            //var allDevice = _pushNotificationRepository.Fetch(x => x.Device == TipoDispositivo.Android && x.Produzione == produzione && x.Validated == true && (x.Language == language || language == "All"));
            var allDevice = GetListMobileDevice(queryDevice, TipoDispositivo.Android, produzione, language);

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
            foreach (PushNotificationRecord pnr in allDevice) {
                //  PushAndroid(pnr, produzione, JsonMessage);
                push.QueueNotification(new GcmNotification().ForDeviceRegistrationId(pnr.Token)
                .WithJson(JsonMessage));
            }
            //   .WithJson(" {    \"tipo\": \"aio\",    \"id\": \"2\",    \"titolo\": \"ole\"  }"));
            push.StopAllServices();
        }

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

        private void SendAllApplePart(MobilePushPart mpp, Int32 idcontent, Int32 idContentRelated, string language, string queryDevice) {
            string ctype = "";
            string displayalias = "";
            var extra = getextrainfo(idContentRelated > 0 ? idContentRelated : idcontent);
            ctype = extra[0];
            displayalias = extra[1];
            PushAppleVM newpush = new PushAppleVM();
            newpush = GenerateApplePush(mpp, idcontent, idContentRelated, ctype, displayalias);
            bool produzione = true;
            if (_orchardServices.WorkContext.CurrentSite.As<PushMobileSettingsPart>().ShowTestOptions)
                produzione = !(mpp.TestPush);
            SendAllApple(newpush, produzione, language, queryDevice);
        }

        private void SendAllApple(PushAppleVM newpush, bool produzione, string language, string queryDevice = "") {
            var allDevice = GetListMobileDevice(queryDevice, TipoDispositivo.Apple, produzione, language);
            //   var allDevice = _pushNotificationRepository.Fetch(x => x.Device == TipoDispositivo.Apple && x.Produzione == produzione && x.Validated == true && (x.Language == language || language == "All"));
            // PushAppleVM testpayloadsize = GenerateApplePush(mpp, idcontent, idContentRelated);
            if (newpush.ValidPayload) {
                PushApple(allDevice.ToList(), produzione, newpush);
            }
        }

        private void SendAllWindowsMobilePart(MobilePushPart mpp, Int32 idcontent, Int32 idContentRelated, string language, string queryDevice) {
            string message = JsonConvert.SerializeObject(GenerateWindowsMobilePush(mpp, idcontent, idContentRelated));
            bool produzione = true;
            if (_orchardServices.WorkContext.CurrentSite.As<PushMobileSettingsPart>().ShowTestOptions)
                produzione = !(mpp.TestPush);
            SendAllWindowsMobile(message, produzione, language, queryDevice);
        }

        private void SendAllWindowsMobile(string message, bool produzione, string language, string queryDevice = "") {
            var allDevice = GetListMobileDevice(queryDevice, TipoDispositivo.WindowsMobile, produzione, language);
            //var allDevice = _pushNotificationRepository.Fetch(x => x.Device == TipoDispositivo.WindowsMobile && x.Produzione == produzione && x.Validated == true && (x.Language == language || language == "All"));
            foreach (PushNotificationRecord pnr in allDevice) {
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
            PushAppleVM mypush = new PushAppleVM();
            mypush.Title = mpp.TitlePush;
            mypush.Text = mpp.TextPush;
            mypush.idContent = idcontent;
            mypush.idRelated = idContentRelated;
            mypush.Ct = ctype;
            mypush.Al = displayalias;
            var partSettings = mpp.Settings.GetModel<PushMobilePartSettingVM>();
            if (!(partSettings.AcceptZeroRelated) && mypush.idRelated == 0)
                mypush.idRelated = mypush.idContent;
            mypush.ValidPayload = true;
            AppleNotification appleNotification = new AppleNotification();
            appleNotification.ForDeviceToken("TokenVirtualePerTestSuPayloadDimension")
            .WithAlert(mypush.Text)
                //        .WithCustomItem("Title", mypush.Title)
            .WithCustomItem("Id", mypush.idContent)
            .WithCustomItem("Rid", mypush.idRelated)
            .WithCustomItem("Ct", mypush.Ct)
            .WithCustomItem("Al", mypush.Al)
            .WithSound(mypush.Sound);
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

        private void PushWindowsMobile(PushNotificationRecord dispositivo, bool produzione, string message) {
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

        private void PushApple(List<PushNotificationRecord> listdispositivo, bool produzione, PushAppleVM pushMessage) {
            //  string AppleCertificateTenant = _shellSetting.Name;
            if (string.IsNullOrWhiteSpace(pushMessage.Sound))
                pushMessage.Sound = _orchardServices.WorkContext.CurrentSite.As<PushMobileSettingsPart>().ApplePushSound;
            if (string.IsNullOrWhiteSpace(pushMessage.Sound))
                pushMessage.Sound = "sound.caf"; //default
            string setting_password = "";
            string setting_file = "";
            if (produzione) {
                setting_password = _orchardServices.WorkContext.CurrentSite.As<PushMobileSettingsPart>().AppleCertificatePassword;
                setting_file = HostingEnvironment.MapPath("~/") + @"App_Data\Sites\" + _shellSetting.Name + @"\Mobile\" + _orchardServices.WorkContext.CurrentSite.As<PushMobileSettingsPart>().ApplePathCertificateFile;
            }
            else {
                setting_password = _orchardServices.WorkContext.CurrentSite.As<PushMobileSettingsPart>().AppleCertificatePasswordDevelopment;
                setting_file = HostingEnvironment.MapPath("~/") + @"App_Data\Sites\" + _shellSetting.Name + @"\Mobile\" + _orchardServices.WorkContext.CurrentSite.As<PushMobileSettingsPart>().ApplePathCertificateFileDevelopment;
            }
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

            foreach (PushNotificationRecord dispositivo in listdispositivo) {
                AppleNotification appleNotification = new AppleNotification();
                appleNotification.ForDeviceToken(dispositivo.Token)
                .WithAlert(pushMessage.Text)
                    //  .WithCustomItem("Title", pushMessage.Title)
                .WithCustomItem("Id", pushMessage.idContent)
                .WithCustomItem("Rid", pushMessage.idRelated)
                .WithCustomItem("Ct", pushMessage.Ct)
                .WithCustomItem("Al", pushMessage.Al)
                .WithSound(pushMessage.Sound);

                if (appleNotification.Payload.ToJson().Length > 255) {
                    _notifier.Information(T("Sent: message payload exceed the limit"));
                }
                else {
                    push.QueueNotification(appleNotification);
                }
            }
            push.StopAllServices();
        }

        private void PushApple(PushNotificationRecord dispositivo, PushAppleVM pushMessage) {
            List<PushNotificationRecord> listdispositivo = new List<PushNotificationRecord>();
            listdispositivo.Add(dispositivo);
            PushApple(listdispositivo, dispositivo.Produzione, pushMessage);
        }

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
            _myLog.WriteLog(T("Sent: " + sender + " -> " + notification).ToString());
            messageAppleSent++;
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