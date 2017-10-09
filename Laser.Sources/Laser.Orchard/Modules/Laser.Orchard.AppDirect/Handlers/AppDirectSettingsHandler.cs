using Laser.Orchard.AppDirect.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Localization;

namespace Laser.Orchard.AppDirect.Handlers {
    public class AppDirectSettingsHandler : ContentHandler {
        public AppDirectSettingsHandler(IRepository<AppDirectSettingsPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
            Filters.Add(new ActivatingFilter<AppDirectSettingsPart>("Site"));
            T = NullLocalizer.Instance;
            OnGetContentItemMetadata<AppDirectSettingsPart>((context, part) => context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("AppDirect"))));
        }
        public Localizer T { get; set; }
    }
}