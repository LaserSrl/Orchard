using Laser.Orchard.ChartaWS.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;

namespace Laser.Orchard.ChartaWS.Drivers
{
    public class ChartaSiteSettingsPartDriver : ContentPartDriver<ChartaSiteSettingsPart>
    {
        private const string TemplateName = "Parts/ChartaSiteSettings";

        public ChartaSiteSettingsPartDriver()
        {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override string Prefix { get { return "ChartaSettings"; } }

        protected override DriverResult Editor(ChartaSiteSettingsPart part, dynamic shapeHelper)
        {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(ChartaSiteSettingsPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            return ContentShape("Parts_ChartaSiteSettings_Edit", () => {
                if (updater != null) {
                    updater.TryUpdateModel(part, Prefix, null, null);
                }
                return shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix);
            })
                .OnGroup("Charta WS");
        }
    }
}