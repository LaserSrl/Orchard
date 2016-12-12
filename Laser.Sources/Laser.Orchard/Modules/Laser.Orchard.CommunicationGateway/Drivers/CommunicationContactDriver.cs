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
using Orchard.ContentManagement;
using Orchard.Users.Models;

namespace Laser.Orchard.CommunicationGateway.Drivers {
    public class CommunicationContactDriver : ContentPartDriver<CommunicationContactPart> {
        private readonly IOrchardServices _orchardServices;
        private readonly IContentManager _contentManager;

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        protected override string Prefix {
            get { return "Laser.Orchard.CommunicationGateway"; }
        }

        public CommunicationContactDriver(IOrchardServices orchardServices, IContentManager contentManager) {
            _orchardServices = orchardServices;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
            _contentManager = contentManager;
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
            
            //mod 12-12-2016
            context.ImportAttribute(part.PartDefinition.Name, "UserIdentifier", x => {
                var tempPartFromid = context.GetItemFromSession(x);

                if (tempPartFromid != null && tempPartFromid.Is<UserPart>()) {
                    //associa id user
                    part.UserIdentifier = tempPartFromid.As<UserPart>().Id;
                }
            });


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
            var root = context.Element(part.PartDefinition.Name);
            
            //mod. 12-12-2016
            if (part.UserIdentifier > 0) {
                //cerco il corrispondente valore dell' identity dalla partse lo associo al campo iduser 
                var contItemUser = _contentManager.Get(part.UserIdentifier);
                if (contItemUser != null) {
                    root.SetAttributeValue("IdUser", _contentManager.GetItemMetadata(contItemUser).Identity.ToString());
                }
            }

            context.Element(part.PartDefinition.Name).SetAttributeValue("Master", part.Master);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Logs", part.Logs);
        
        }
    }
}