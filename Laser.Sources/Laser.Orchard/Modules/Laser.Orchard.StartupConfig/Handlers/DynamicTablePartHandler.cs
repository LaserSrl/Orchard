using Laser.Orchard.StartupConfig.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.StartupConfig.Handlers {
    [OrchardFeature("Laser.Orchard.StartupConfig.DynamicTablePart")]
    public class DynamicTablePartHandler : ContentHandler {
        public DynamicTablePartHandler(IRepository<DynamicTablePartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}