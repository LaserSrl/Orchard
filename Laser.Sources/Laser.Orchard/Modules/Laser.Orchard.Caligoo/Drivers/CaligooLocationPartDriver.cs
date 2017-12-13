using Laser.Orchard.Caligoo.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace Laser.Orchard.Caligoo.Drivers {
    public class CaligooLocationPartDriver : ContentPartDriver<CaligooLocationPart> {
        protected override DriverResult Display(CaligooLocationPart part, string displayType, dynamic shapeHelper) {
            return ContentShape("Parts_CaligooLocation", () => shapeHelper.Parts_CaligooLocation());
        }
        protected override DriverResult Editor(CaligooLocationPart part, dynamic shapeHelper) {
            return ContentShape("Parts_CaligooLocation_Edit", () => shapeHelper.EditorTemplate(
                TemplateName: "Parts/CaligooLocation",
                Model: part,
                Prefix: Prefix));
        }
        protected override DriverResult Editor(CaligooLocationPart part, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }
    }
}