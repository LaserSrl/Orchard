using Laser.Orchard.CommunicationGateway.Models;
using Orchard.ContentManagement.Drivers;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.CommunicationGateway.Drivers {
    public class SmsContactPartDriver : ContentPartDriver<SmsContactPart> {

        protected override string Prefix {
            get { return "Laser.Mobile.SmsContact"; }
        }

        protected override DriverResult Editor(SmsContactPart part, dynamic shapeHelper) {
            List<CommunicationSmsRecord> viewModel = part.SmsEntries.Value.ToList();
            return ContentShape("Parts_SmsContact_Edit", () => shapeHelper.EditorTemplate(TemplateName: "Parts/SmsContact_Edit", Model: viewModel, Prefix: Prefix));
        }
    }

}