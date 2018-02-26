using Laser.Orchard.Caligoo.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;

namespace Laser.Orchard.Caligoo.Drivers {
    public class CaligooSiteSettingsDriver : ContentPartDriver<CaligooSiteSettingsPart> {
        private const string TemplateName = "Parts/CaligooSiteSettings";
        public Localizer T { get; set; }
        protected override string Prefix { get { return "CaligooSettings"; } }
        public CaligooSiteSettingsDriver() {
            T = NullLocalizer.Instance;
        }
        protected override DriverResult Editor(CaligooSiteSettingsPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }
        protected override DriverResult Editor(CaligooSiteSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            return ContentShape("Parts_CaligooSiteSettings_Edit", () => {
                if (updater != null) {
                    updater.TryUpdateModel(part, Prefix, null, null);
                }
                return shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix);
            }).OnGroup("Caligoo");
        }
    }
}