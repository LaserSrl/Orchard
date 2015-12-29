using Laser.Orchard.CommunicationGateway.Models;
using Orchard.ContentManagement.Drivers;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.CommunicationGateway.Drivers {
    public class EmailContactPartDriver : ContentPartDriver<EmailContactPart> {
   
        protected override string Prefix {
            get { return "Laser.Mobile.EmailContact"; }
        }

        protected override DriverResult Editor(EmailContactPart part, dynamic shapeHelper) {
            List<CommunicationEmailRecord> viewModel = part.EmailEntries.Value.ToList();
            return ContentShape("Parts_EmailContact_Edit", () => shapeHelper.EditorTemplate(TemplateName: "Parts/EmailContact_Edit", Model: viewModel, Prefix: Prefix));
         }
    }

}