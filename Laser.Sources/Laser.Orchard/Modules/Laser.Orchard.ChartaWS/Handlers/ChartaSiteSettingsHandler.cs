using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.ChartaWS.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.Logging;

namespace Laser.Orchard.ChartaWS.Handlers
{
    public class ChartaSiteSettingsHandler : ContentHandler
    {
        public ChartaSiteSettingsHandler()
        {
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
            Filters.Add(new ActivatingFilter<ChartaSiteSettingsPart>("Site"));
        }

        public Localizer T { get; set; }
        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            if (context.ContentItem.ContentType != "Site")
            {
                return;
            }
            base.GetItemMetadata(context);
            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("Charta WS")));
        }
    }
}