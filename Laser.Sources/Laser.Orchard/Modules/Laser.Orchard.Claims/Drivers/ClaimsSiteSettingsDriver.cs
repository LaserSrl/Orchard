using Laser.Orchard.Claims.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Claims.Drivers {
    public class ClaimsSiteSettingsDriver : ContentPartDriver<ClaimsSiteSettings> {
        private const string TemplateName = "Parts/ClaimsSiteSettings";

        public ClaimsSiteSettingsDriver() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override string Prefix { get { return "ClaimsSiteSettings"; } }

        protected override DriverResult Editor(ClaimsSiteSettings part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(ClaimsSiteSettings part, IUpdateModel updater, dynamic shapeHelper) {
            return ContentShape("Parts_ClaimsSiteSettings_Edit", () => {
                if (updater != null) {
                    updater.TryUpdateModel(part, Prefix, null, null);
                }
                return shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix);
            })
                .OnGroup("Claims");
        }
    }
}