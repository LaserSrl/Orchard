using Laser.Orchard.CommunicationGateway.Models;
using Laser.Orchard.Mobile.Models;
using Orchard.ContentManagement.Drivers;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Mobile.Drivers {
    public class MobileContactPartDriver : ContentPartDriver<MobileContactPart> {
     //   private readonly IRepository<PushNotificationRecord> _repositoryPushNotificationRecord;
        protected override string Prefix {
            get { return "Laser.Mobile.MobileContact"; }
        }

        //public MobileContactPartDriver(IRepository<PushNotificationRecord> repositoryPushNotificationRecord) {
        //    _repositoryPushNotificationRecord =  repositoryPushNotificationRecord;
        //}

        protected override DriverResult Editor(MobileContactPart part, dynamic shapeHelper) {
            List<PushNotificationRecord> viewModel = part.MobileEntries.Value.ToList();
            return ContentShape("Parts_MobileContact_Edit", () => shapeHelper.EditorTemplate(TemplateName: "Parts/MobileContact_Edit", Model: viewModel, Prefix: Prefix));
         }
    }

}