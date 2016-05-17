using Laser.Orchard.CommunicationGateway.Models;
using Orchard;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.Logging;
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