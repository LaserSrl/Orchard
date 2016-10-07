using Laser.Orchard.CommunicationGateway.Models;
using Laser.Orchard.CommunicationGateway.ViewModels;
using Orchard;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.UI.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.CommunicationGateway.Drivers {
    public class CommunicationContactDriver : ContentPartDriver<CommunicationContactPart> {
        private readonly IOrchardServices _orchardServices;

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        protected override string Prefix {
            get { return "Laser.Orchard.CommunicationGateway"; }
        }

        public CommunicationContactDriver(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(CommunicationContactPart part, string displayType, dynamic shapeHelper) {
            //Determine if we're on an admin page
            bool isAdmin = AdminFilter.IsApplied(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
            if (isAdmin) {
                if (displayType == "Detail") {
                    string logs = T("No log.").Text;
                    if (string.IsNullOrWhiteSpace(part.Logs) == false) {
                        logs = part.Logs;
                    }
                    var profile = part.ContentItem.Parts.FirstOrDefault(x => x.PartDefinition.Name == "ProfilePart");
                    return Combined(ContentShape("Parts_CommunicationContact",
                        () => shapeHelper.Parts_CommunicationContact(Logs: logs)),
                        ContentShape("Parts_ProfilePart",
                        () => shapeHelper.Parts_ProfilePart(ContentPart: profile))
                            );
                } else {
                    return null;
                }
            } else {
                return null;
            }
        }

        protected override DriverResult Editor(CommunicationContactPart part, dynamic shapeHelper) {
            CommunicationContactPartVM model = new CommunicationContactPartVM();
            if (string.IsNullOrWhiteSpace(part.Logs)) {
                model.Logs = T("No log.").Text;
            } else {
                model.Logs = part.Logs;
            }
            return ContentShape("Parts_CommunicationContact_Edit", () => shapeHelper.EditorTemplate(TemplateName: "Parts/CommunicationContact_Edit", Model: model, Prefix: Prefix));
        }

        protected override void Importing(CommunicationContactPart part, ImportContentContext context) {
            //var root = context.Data.Element(part.PartDefinition.Name);
            //part.UserIdentifier = int.Parse(root.Attribute("UserIdentifier").Value);
            //part.Master = bool.Parse(root.Attribute("Master").Value);
            //part.Logs = root.Attribute("Logs").Value;

            var importedUserIdentifier = context.Attribute(part.PartDefinition.Name, "UserIdentifier");
            if (importedUserIdentifier != null) {
                part.UserIdentifier = Convert.ToInt32(importedUserIdentifier);
            }

            var importedMaster = context.Attribute(part.PartDefinition.Name, "Master");
            if (importedMaster != null) {
                part.Master = Convert.ToBoolean(importedMaster);
            }

            var importedLogs = context.Attribute(part.PartDefinition.Name, "Logs");
            if (importedLogs != null) {
                part.Logs = importedLogs;
            }

        }

        protected override void Exporting(CommunicationContactPart part, ExportContentContext context) {
            //var root = context.Element(part.PartDefinition.Name);
            //root.SetAttributeValue("UserIdentifier", part.UserIdentifier);
            //root.SetAttributeValue("Master", part.Master);
            //root.SetAttributeValue("Logs", part.Logs);
            context.Element(part.PartDefinition.Name).SetAttributeValue("UserIdentifier", part.UserIdentifier);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Master", part.Master);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Logs", part.Logs);
        
        }
    }
}