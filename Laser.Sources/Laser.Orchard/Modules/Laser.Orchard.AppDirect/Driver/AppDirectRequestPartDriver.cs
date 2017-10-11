using System;
using System.Linq;
using Laser.Orchard.AppDirect.Models;
using Laser.Orchard.AppDirect.Services;
using Laser.Orchard.AppDirect.ViewModels;
using Newtonsoft.Json.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Data;
using Orchard.Localization;

namespace Laser.Orchard.AppDirect.Driver {
    public class AppDirectRequestPartDriver : ContentPartDriver<AppDirectRequestPart> {
        public AppDirectRequestPartDriver() {
            T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }
        protected override string Prefix {
            get { return "Laser.Orchard.AppDirect.AppDirectRequestPart"; }
        }

        protected override DriverResult Editor(AppDirectRequestPart part, dynamic shapeHelper) {
            return ContentShape("Parts_AppDirectRequestPart",
                               () => shapeHelper.EditorTemplate(
                                   TemplateName: "Parts/AppDirectRequestPart",
                                   Model: part,
                                   Prefix: Prefix));
        }
    }
}