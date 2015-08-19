using Laser.Orchard.Queues.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;

namespace Laser.Orchard.Queues.Handlers
{
    public class QueuesSettingsHandler : ContentHandler
    {
        public Localizer T { get; set; }

        public QueuesSettingsHandler()
        {
            T = NullLocalizer.Instance;
            Filters.Add(new ActivatingFilter<QueuesSettingsPart>("Site"));
        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context)
        {
            if (context.ContentItem.ContentType != "Site")
                return;

            base.GetItemMetadata(context);
            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("Queues")));
        }
    }
}