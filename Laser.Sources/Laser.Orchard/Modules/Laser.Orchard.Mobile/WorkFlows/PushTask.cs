using Laser.Orchard.Mobile.Models;
using Laser.Orchard.Mobile.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;
using System;
using System.Collections.Generic;

namespace Laser.Orchard.Mobile.WorkFlows {
    [OrchardFeature("Laser.Orchard.PushGateway")]
    public class PushTask : Task {
        private readonly IOrchardServices _orchardServices;
        private readonly IPushGatewayService _pushGatewayService;
        private readonly IRepository<UserDeviceRecord> _userDeviceRecord;

        public PushTask(
            IOrchardServices orchardServices,
            IPushGatewayService pushGatewayService,
            IRepository<UserDeviceRecord> userDeviceRecord
            ) {
            _pushGatewayService = pushGatewayService;
            _orchardServices = orchardServices;
            _userDeviceRecord = userDeviceRecord;
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
            _pushGatewayService.SendPushService(produzione, device, idRelated, language, messageApple, messageAndroid, messageWindows, sound, querydevice);

            yield return T("Sent");
        }

        private static IEnumerable<string> SplitEmail(string commaSeparated) {
            if (commaSeparated == null) return null;
            return commaSeparated.Split(new[] { ',', ';' });
        }
    }
}