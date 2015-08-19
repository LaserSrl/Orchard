using Laser.Orchard.Mobile.Models;
using Laser.Orchard.Mobile.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;
using System;
using System.Collections.Generic;

//using Orchard.Email.Models;
namespace Laser.Orchard.Mobile.WorkFlows {

    //[OrchardFeature("Laser.Orchard.MobileActivities")]
    public class PushTask : Task {
        private readonly IOrchardServices _orchardServices;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly IRepository<UserDeviceRecord> _userDeviceRecord;
        //private readonly IMessageManager _messageManager;
        //private readonly IMembershipService _membershipService;
        //private readonly ITemplateService _templateServices;

        //      public const string MessageType = "ActionTemplatedEmail";\

        public PushTask(
            //IMessageManager messageManager,
            IOrchardServices orchardServices,
            IPushNotificationService pushNotificationService,
            IRepository<UserDeviceRecord> userDeviceRecord
            //IMembershipService membershipService,
            //ITemplateService templateServices
            ) {
            //_messageManager = messageManager;
            _pushNotificationService = pushNotificationService;
            _orchardServices = orchardServices;
            _userDeviceRecord = userDeviceRecord;
            //_membershipService = membershipService;
            //_templateServices = templateServices;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            return new[] { T("Sent") };
        }

        public override string Form {
            get {
                return "ActivityMobileForm";
            }
        }

        public override LocalizedString Category {
            get { return T("Messaging"); }
        }

        public override string Name {
            get { return "SendPush"; }
        }

        public override LocalizedString Description {
            get { return T("Send Push."); }
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            //var FormCollection = _orchardServices.WorkContext.HttpContext.Request.Form;
            ContentItem contentItem = workflowContext.Content.ContentItem;
            var device = activityContext.GetState<string>("allDevice");
            var PushMessage = activityContext.GetState<string>("PushMessage");
            bool produzione = activityContext.GetState<string>("Produzione") == "Produzione";

            Int32 idRelated = 0;
            if (activityContext.GetState<string>("idRelated") == "idRelated") {
                idRelated = contentItem.Id;
            }
            string language = activityContext.GetState<string>("allLanguage");
            string messageApple = PushMessage;
            string messageAndroid = PushMessage;
            string messageWindows = PushMessage;
            string jsonAndroid = "";
            string sound = "";
            string querydevice = "";
            if (device == "ContentOwner") {
                querydevice = " SELECT  distinct P.* " +
                                    " FROM  Laser_Orchard_Mobile_PushNotificationRecord AS P " +
                                    " LEFT OUTER JOIN Laser_Orchard_Mobile_UserDeviceRecord AS U ON P.UUIdentifier = U.UUIdentifier " +
                                    " Where U.UserPartRecord_Id=" + contentItem.As<CommonPart>().Owner.Id.ToString();
                device = "All";
            }
            if (device == "ContentCreator") {
                querydevice = " SELECT  distinct P.* " +
                                    " FROM  Laser_Orchard_Mobile_PushNotificationRecord AS P " +
                                    " LEFT OUTER JOIN Laser_Orchard_Mobile_UserDeviceRecord AS U ON P.UUIdentifier = U.UUIdentifier " +
                                    " Where U.UserPartRecord_Id=" + ((dynamic)contentItem.As<CommonPart>()).Creator.Value.ToString();
     
                device = "All";
            }
            if (device == "ContentLastModifier") {
                querydevice = " SELECT  distinct P.* " +
                                    " FROM  Laser_Orchard_Mobile_PushNotificationRecord AS P " +
                                    " LEFT OUTER JOIN Laser_Orchard_Mobile_UserDeviceRecord AS U ON P.UUIdentifier = U.UUIdentifier " +
                                    " Where U.UserPartRecord_Id=" + ((dynamic)contentItem.As<CommonPart>()).LastModifier.Value.ToString();

                device = "All";
            }
            //else {
            //    //Int32 idRelated = Convert.ToInt32(FormCollection.Get("IdRelated"));
            //    //string language = FormCollection.Get("Language");
            //    //string messageApple = FormCollection.Get("MessageApple");
            //    //string messageAndroid = FormCollection.Get("MessageAndroid");
            //    //string jsonAndroid = FormCollection.Get("JsonAndroid");
            //    //string messageWindows = FormCollection.Get("MessageWindows");
            //    //string sound = FormCollection.Get("Sound");
            //    //string device = activityContext.GetState<string>("Device");
            //    //string PushMessage = activityContext.GetState<string>("PushMessage");
            //    //bool produzione = bool.Parse(activityContext.GetState<string>("Produzione"));
            //    //Int32 idRelated = Convert.ToInt32(activityContext.GetState<string>("IdRelated"));
            //    //string language = activityContext.GetState<string>("Language");
            //    //string messageApple = activityContext.GetState<string>("MessageApple");
            //    //string messageAndroid = activityContext.GetState<string>("MessageAndroid");
            //    //string jsonAndroid = activityContext.GetState<string>("JsonAndroid");
            //    //string messageWindows = activityContext.GetState<string>("MessageWindows");
            //    //string sound = activityContext.GetState<string>("Sound");
            //    //var properties = new Dictionary<string, string> {
            //    //    {"Body", activityContext.GetState<string>("Body")},
            //    //    {"Subject", activityContext.GetState<string>("Subject")},
            //    //    {"RecipientOther",activityContext.GetState<string>("RecipientOther")},
            //    //    {"EmailTemplate",activityContext.GetState<string>("EmailTemplate")}
            //    //};
            //   // List<string> sendTo = new List<string>();

            //    //  int.TryParse(properties["EmailTemplate"], out templateId);
            //    //dynamic contentModel = new {
            //    //    ContentItem = workflowContext.Content,
            //    //    FormCollection = _orchardServices.WorkContext.HttpContext.Request.Form
            //    //};

            //    _pushNotificationService.SendPushService(produzione, device, idRelated, language, messageApple, messageAndroid, jsonAndroid, messageWindows, sound);

            //    //if (device == "All") {
            //    //    var content = workflowContext.Content;
            //    //    if (content.Has<CommonPart>()) {
            //    //        var owner = content.As<CommonPart>().Owner;
            //    //        if (owner != null && owner.ContentItem != null && owner.ContentItem.Record != null) {
            //    //            sendTo.AddRange(SplitEmail(owner.As<IUser>().Email));
            //    //            // _messageManager.Send(owner.ContentItem.Record, MessageType, "email", properties);
            //    //        }
            //    //        sendTo.AddRange(SplitEmail(owner.As<IUser>().Email));
            //    //    }
            //    //} else if (device == "Android") {
            //    //    var user = _orchardServices.WorkContext.CurrentUser;

            //    //    // can be null if user is anonymous
            //    //    if (user != null && !String.IsNullOrWhiteSpace(user.Email)) {
            //    //        sendTo.AddRange(SplitEmail(user.Email));
            //    //    }
            //    //} else if (device == "Apple") {
            //    //    var username = _orchardServices.WorkContext.CurrentSite.SuperUser;
            //    //    //var user = _membershipService.GetUser(username);

            //    //    //   // can be null if user is no super user is defined
            //    //    //   if (user != null && !String.IsNullOrWhiteSpace(user.Email)) {
            //    //    //       sendTo.AddRange(SplitEmail(user.As<IUser>().Email));
            //    //    //   }
            //    //} else if (device == "WindowsMobile") {
            //    //    sendTo.AddRange(SplitEmail(activityContext.GetState<string>("RecipientOther")));
            //    //}
            //    //   SendEmail(contentModel, templateId, sendTo, null);
            //    //      _pushNotificationService.PublishedPushEvent(contentModel, templateId, sendTo, null);
            //}
            _pushNotificationService.SendPushService(produzione, device, idRelated, language, messageApple, messageAndroid, jsonAndroid, messageWindows, sound, querydevice);

            yield return T("Sent");
        }

        private static IEnumerable<string> SplitEmail(string commaSeparated) {
            if (commaSeparated == null) return null;
            return commaSeparated.Split(new[] { ',', ';' });
        }

        private void SendEmail(dynamic contentModel, int templateId, IEnumerable<string> sendTo, IEnumerable<string> bcc) {
            //ParseTemplateContext templatectx = new ParseTemplateContext();
            //var template = _templateServices.GetTemplate(templateId);
            //var urlHelper = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);

            //// Creo un model che ha Content (il contentModel), Urls con alcuni oggetti utili per il template
            //// Nel template pertanto Model, diventa Model.Content
            //var host = string.Format("{0}://{1}{2}",
            //                        _orchardServices.WorkContext.HttpContext.Request.Url.Scheme,
            //                        _orchardServices.WorkContext.HttpContext.Request.Url.Host,
            //                        _orchardServices.WorkContext.HttpContext.Request.Url.Port == 80
            //                            ? string.Empty
            //                            : ":" + _orchardServices.WorkContext.HttpContext.Request.Url.Port);
            //var dynamicModel = new {
            //    Content = contentModel,
            //    Urls = new {
            //        MediaUrl = urlHelper.MediaExtensionsImageUrl(),
            //        Domain = host,

            //    }.ToExpando()
            //};
            //templatectx.Model = dynamicModel;
            //var body = _templateServices.ParseTemplate(template, templatectx);
            //var data = new Dictionary<string, string>();
            //data.Add("Subject", template.Subject);
            //data.Add("Body", body);
            //if (bcc != null) {
            //    data.Add("Bcc", String.Join(",", bcc));
            //}
            //var smtp = _orchardServices.WorkContext.CurrentSite.As<SmtpSettingsPart>();
            //var recipient = sendTo != null ? sendTo : new List<string> { smtp.Address };
            //_messageManager.Send(recipient, "ActionEmail", "email", data);
        }
    }
}