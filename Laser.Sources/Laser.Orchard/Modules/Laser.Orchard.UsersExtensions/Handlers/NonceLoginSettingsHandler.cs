using Laser.Orchard.UsersExtensions.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;

namespace Laser.Orchard.UsersExtensions.Handlers {
    public class NonceLoginSettingsHandler : ContentHandler {
        public Localizer T { get; set; }
        public NonceLoginSettingsHandler() {
            T = NullLocalizer.Instance;
            Filters.Add(new ActivatingFilter<NonceLoginSettingsPart>("Site"));
        }
        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            if (context.ContentItem.ContentType != "Site") {
                return;
            }
            base.GetItemMetadata(context);
            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("Nonce Login")));
        }
    }
}