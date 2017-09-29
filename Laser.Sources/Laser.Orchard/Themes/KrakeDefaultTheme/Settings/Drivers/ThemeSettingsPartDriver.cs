using KrakeDefaultTheme.Settings.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;

namespace KrakeDefaultTheme.Settings.Drivers {
    public class ThemeSettingsPartDriver : ContentPartDriver<ThemeSettingsPart> {
        private const string TEMPLATENAME = "Parts/ThemeSettings";
        private const string SHAPENAME = "Parts_ThemeSettings_Edit";

        public ThemeSettingsPartDriver() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override string Prefix { get { return "ThemeSettings"; } }

        protected override DriverResult Editor(ThemeSettingsPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(ThemeSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            return ContentShape(SHAPENAME, () => {
                if (updater != null) {
                    updater.TryUpdateModel(part, Prefix, null, null);
                }
                return shapeHelper.EditorTemplate(TemplateName: TEMPLATENAME, Model: part, Prefix: Prefix);
            })
                .OnGroup("Krake");
        }
    }
}