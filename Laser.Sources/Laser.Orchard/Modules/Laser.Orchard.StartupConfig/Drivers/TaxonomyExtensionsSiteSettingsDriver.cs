using Laser.Orchard.StartupConfig.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.StartupConfig.Drivers
{
    [OrchardFeature("Laser.Orchard.StartupConfig.TaxonomiesExtensions")]
    public class TaxonomyExtensionsSiteSettingsDriver : ContentPartDriver<TaxonomyExtensionsSiteSettingsPart>
    {
        private const string TemplateName = "Parts/TaxonomyExtensionsSiteSettings";

        protected override string Prefix { get { return "TaxonomyExtensionsSiteSettings"; } }

        //GET
        protected override DriverResult Editor(TaxonomyExtensionsSiteSettingsPart part, dynamic shapeHelper)
        {

            return ContentShape("Parts_TaxonomyExtensionsSiteSettings_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: TemplateName,
                    Model: part,
                    Prefix: Prefix));
        }

        //POST
        protected override DriverResult Editor(TaxonomyExtensionsSiteSettingsPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }
    }
}