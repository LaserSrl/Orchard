using Laser.Orchard.Caligoo.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.Logging;

namespace Laser.Orchard.Caligoo.Handlers {
    public class CaligooSiteSettingsHandler : ContentHandler {
        public Localizer T { get; set; }
        public CaligooSiteSettingsHandler() {
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
            Filters.Add(new ActivatingFilter<CaligooSiteSettingsPart>("Site"));
        }
        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            if (context.ContentItem.ContentType != "Site") {
                return;
            }
            base.GetItemMetadata(context);
            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("Caligoo")));
        }
    }
}