using Laser.Orchard.UsersExtensions.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;

namespace Laser.Orchard.UsersExtensions.Drivers {
    public class NonceLoginSettingsDriver : ContentPartDriver<NonceLoginSettingsPart> {
        private const string TemplateName = "Parts/NonceLoginSettings";
        public Localizer T { get; set; }
        protected override string Prefix { get { return "NonceLoginSettings"; } }

        public NonceLoginSettingsDriver() {
            T = NullLocalizer.Instance;
        }
        protected override DriverResult Editor(NonceLoginSettingsPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }
        protected override DriverResult Editor(NonceLoginSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            return ContentShape("Parts_NonceLoginSettings_Edit", () => {
                if (updater != null) {
                    updater.TryUpdateModel(part, Prefix, null, null);
                }
                return shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix);
            }).OnGroup("NonceLogin");
        }
    }
}