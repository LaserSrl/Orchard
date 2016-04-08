using Laser.Orchard.Mobile.Models;
using Orchard.ContentManagement.Drivers;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.Mobile.Drivers {

    public class MobileContactPartDriver : ContentPartDriver<MobileContactPart> {

        protected override string Prefix {
            get { return "Laser.Mobile.MobileContact"; }
        }

        protected override DriverResult Editor(MobileContactPart part, dynamic shapeHelper) {
            List<PushNotificationRecord> viewModel = new List<PushNotificationRecord>();
            if (part.MobileEntries != null && part.MobileEntries.Value != null && part.MobileEntries.Value.Count > 0)
                viewModel = part.MobileEntries.Value.ToList();
            return ContentShape("Parts_MobileContact_Edit", () => shapeHelper.EditorTemplate(TemplateName: "Parts/MobileContact_Edit", Model: viewModel, Prefix: Prefix));
        }
    }
}