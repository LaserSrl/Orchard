using Laser.Orchard.AppDirect.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace Laser.Orchard.AppDirect.Driver {

    public class AppDirectUserDriver : ContentPartDriver<AppDirectUserPart> {
        protected override string Prefix {
            get { return "Laser.Orchard.AppDirect.AppDirectUserPart"; }
        }
        protected override DriverResult Editor(AppDirectUserPart part, dynamic shapeHelper) {
            return ContentShape("Parts_AppDirectUser",
                               () => shapeHelper.EditorTemplate(
                                   TemplateName: "Parts/AppDirectUser",
                                   Model: part,
                                   Prefix: Prefix));
        }
        protected override DriverResult Editor(AppDirectUserPart part, IUpdateModel updater, dynamic shapeHelper) {
            return ContentShape("Parts_AppDirectUser",
                               () => shapeHelper.EditorTemplate(
                                   TemplateName: "Parts/AppDirectUser",
                                   Model: part,
                                   Prefix: Prefix));
        }
    }
}
