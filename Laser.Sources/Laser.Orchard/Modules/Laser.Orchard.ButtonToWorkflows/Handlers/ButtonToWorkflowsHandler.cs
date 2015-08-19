using Laser.Orchard.ButtonToWorkflows.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Localization;

namespace Laser.Orchard.ButtonToWorkflows.Handlers {
    [OrchardFeature("Laser.Orchard.ButtonToWorkflows")]
    public class SiteSettingsPartHandler : ContentHandler {
        public SiteSettingsPartHandler(IRepository<ButtonToWorkflowsSettingsPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
            Filters.Add(new ActivatingFilter<ButtonToWorkflowsSettingsPart>("Site"));
            T = NullLocalizer.Instance;
            OnGetContentItemMetadata<ButtonToWorkflowsSettingsPart>((context, part) => context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("Buttons"))));
        }

        public Localizer T { get; set; }
    }
}