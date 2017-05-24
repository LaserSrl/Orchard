using Laser.Orchard.Claims.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Claims.Handlers {
    public class ClaimsSiteSettingsHandler : ContentHandler {
        public Localizer T { get; set; }
        public ClaimsSiteSettingsHandler() {
            T = NullLocalizer.Instance;
            Filters.Add(new ActivatingFilter<ClaimsSiteSettings>("Site"));
        }
        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            if (context.ContentItem.ContentType != "Site") {
                return;
            }
            base.GetItemMetadata(context);
            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("Claims")));
        }
    }
}