using Laser.Orchard.CommunicationGateway.Models;
using Laser.Orchard.CommunicationGateway.ViewModels;
using Laser.Orchard.StartupConfig.Services;
using Orchard;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Security;
using Orchard.UI.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.CommunicationGateway.Drivers {
    public class CommunicationContactDriver : ContentPartDriver<CommunicationContactPart> {
        private readonly IOrchardServices _orchardServices;
        private readonly IControllerContextAccessor _controllerContextAccessor;

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        protected override string Prefix {
            get { return "Laser.Orchard.CommunicationGateway"; }
        }

        public CommunicationContactDriver(IOrchardServices orchardServices, IControllerContextAccessor controllerContextAccessor) {
            _orchardServices = orchardServices;
            _controllerContextAccessor = controllerContextAccessor;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(CommunicationContactPart part, string displayType, dynamic shapeHelper) {
            // check sulle permission (esclude il modulo Generator)
            if (_controllerContextAccessor.Context.Controller.GetType().Namespace != "Laser.Orchard.Generator.Controllers") {
                if (_orchardServices.Authorizer.Authorize(Permissions.ShowContacts) == false) {
                    //throw new OrchardSecurityException(T("You do not have permission to access this content."));
                    HttpContext.Current.Response.Clear();
                    HttpContext.Current.Response.SuppressContent = true;
                    HttpContext.Current.Response.StatusCode = 401; //Unauthorized
                    HttpContext.Current.Response.End();
                }
            }
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

        //protected override void Importing(CommunicationContactPart part, ImportContentContext context) {
        //    var root = context.Data.Element(part.PartDefinition.Name);
        //    part.AllDay = Boolean.Parse(root.Attribute("AllDay").Value);
        //    part.DateTimeEnd = root.Attribute("DateTimeEnd").Value != null ? DateTime.Parse(root.Attribute("DateTimeEnd").Value, CultureInfo.InvariantCulture) : (DateTime?)null;
        //    part.DateTimeStart = root.Attribute("DateTimeStart").Value != null ? DateTime.Parse(root.Attribute("DateTimeStart").Value, CultureInfo.InvariantCulture) : (DateTime?)null;
        //    part.Repeat = Boolean.Parse(root.Attribute("Repeat").Value);
        //    part.RepeatDetails = root.Attribute("RepeatDetails").Value;
        //    part.RepeatEnd = Boolean.Parse(root.Attribute("RepeatEnd").Value);
        //    part.RepeatEndDate = root.Attribute("RepeatEndDate").Value != null ? DateTime.Parse(root.Attribute("RepeatEndDate").Value, CultureInfo.InvariantCulture) : (DateTime?)null;
        //    part.RepeatType = root.Attribute("RepeatType").Value;
        //    part.RepeatValue = int.Parse(root.Attribute("RepeatValue").Value, CultureInfo.InvariantCulture);
        //}

        //protected override void Exporting(CommunicationContactPart part, ExportContentContext context) {
        //    var root = context.Element(part.PartDefinition.Name);
        //    root.SetAttributeValue("AllDay", part.AllDay);
        //    root.SetAttributeValue("DateTimeEnd", part.DateTimeEnd.HasValue ? part.DateTimeEnd.Value.ToString(CultureInfo.InvariantCulture) : null);
        //    root.SetAttributeValue("DateTimeStart", part.DateTimeStart.HasValue ? part.DateTimeStart.Value.ToString(CultureInfo.InvariantCulture) : null);
        //    root.SetAttributeValue("Repeat", part.Repeat);
        //    root.SetAttributeValue("RepeatDetails", part.RepeatDetails);
        //    root.SetAttributeValue("RepeatEnd", part.RepeatEnd);
        //    root.SetAttributeValue("RepeatEndDate", part.RepeatEndDate.HasValue ? part.RepeatEndDate.Value.ToString(CultureInfo.InvariantCulture) : null);
        //    root.SetAttributeValue("RepeatType", part.RepeatType);
        //    root.SetAttributeValue("RepeatValue", part.RepeatValue);

        //}
    }
}