using Laser.Orchard.Caligoo.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Caligoo.Drivers {
    public class CaligooUserPartDriver : ContentPartDriver<CaligooUserPart> {
        protected override DriverResult Display(CaligooUserPart part, string displayType, dynamic shapeHelper) {
            return ContentShape("Parts_CaligooUser", () => shapeHelper.Parts_CaligooUser());
        }
        protected override DriverResult Editor(CaligooUserPart part, dynamic shapeHelper) {
            return ContentShape("Parts_CaligooUser_Edit", () => shapeHelper.EditorTemplate(
                TemplateName: "Parts/CaligooUser",
                Model: part,
                Prefix: Prefix));
        }
        protected override DriverResult Editor(CaligooUserPart part, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }
    }
}