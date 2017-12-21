using Laser.Orchard.Caligoo.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Laser.Orchard.Caligoo.Handlers {
    public class CaligooUserPartHandler : ContentHandler {
        public CaligooUserPartHandler(IRepository<CaligooUserPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}