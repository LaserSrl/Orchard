using Laser.Orchard.StartupConfig.Models;
using Orchard.ContentManagement.Handlers;

namespace Laser.Orchard.StartupConfig.TaxonomiesExtensions.Handlers
{
    public class TaxonomyExtensionsSiteSettingsHandler : ContentHandler
    {
        public TaxonomyExtensionsSiteSettingsHandler()
        {
            //OnInitialized<TaxonomyExtensionsSiteSettingsPart>((context, part) => { part.LocalizeTerms = true; });
            Filters.Add(new ActivatingFilter<TaxonomyExtensionsSiteSettingsPart>("Site"));
        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context)
        {
            if (context.ContentItem.ContentType != "Site")
                return;

            base.GetItemMetadata(context);
        }
    }
}