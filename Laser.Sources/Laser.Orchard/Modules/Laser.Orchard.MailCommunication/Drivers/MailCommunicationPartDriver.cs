using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.MailCommunication.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;

namespace Laser.Orchard.MailCommunication.Drivers {
    public class MailCommunicationPartDriver : ContentPartDriver<MailCommunicationPart> {
        private readonly IContentManager _contentManager;
        private readonly IOrchardServices _orchardServices;

        public MailCommunicationPartDriver(IContentManager contentManager, IOrchardServices orchardServices) {
            _contentManager = contentManager;
            _orchardServices = orchardServices;
        }
        protected override string Prefix {
            get { return "MailCommunicationPart"; }
        }

        protected override DriverResult Editor(MailCommunicationPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(MailCommunicationPart part, IUpdateModel updater, dynamic shapeHelper) {
            if (updater != null) {
                if (updater.TryUpdateModel(part, Prefix, null, null)) {
                    if (_orchardServices.WorkContext.HttpContext.Request.Form["submit.Save"] == "submit.MailTest") {
                        // Logica di invio mail forse meglio metterla in un handler > OnUpdated
                    }
                }
            }
            var shapes = new List<DriverResult>();
            shapes.Add(ContentShape("Parts_MailCommunication_Edit", () => shapeHelper.EditorTemplate(TemplateName: "Parts/MailCommunication_Edit", Model: part, Prefix: Prefix)));
            shapes.Add(ContentShape("Parts_MailCommunicationActions_Edit", () => shapeHelper.EditorTemplate(TemplateName: "Parts/MailCommunicationActions_Edit", Model: part, Prefix: Prefix)));

            return new CombinedResult(shapes);
        }

        //TODO: Importing/Exporting 
        protected override void Importing(MailCommunicationPart part, ImportContentContext context) {
        }

        protected override void Exporting(MailCommunicationPart part, ExportContentContext context) {
        }
    }
}