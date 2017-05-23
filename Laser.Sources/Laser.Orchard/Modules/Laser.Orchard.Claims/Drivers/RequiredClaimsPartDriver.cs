using Laser.Orchard.Claims.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Claims.Drivers {
    public class RequiredClaimsPartDriver : ContentPartDriver<RequiredClaimsPart> {
        protected override DriverResult Display(RequiredClaimsPart part, string displayType, dynamic shapeHelper) {
            return null;
        }

        protected override DriverResult Editor(RequiredClaimsPart part, dynamic shapeHelper) {
            return ContentShape("Parts_RequiredClaimsPart_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/RequiredClaimsPart",
                    Model: part,
                    Prefix: Prefix));
        }

        protected override DriverResult Editor(RequiredClaimsPart part, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }
    }
}