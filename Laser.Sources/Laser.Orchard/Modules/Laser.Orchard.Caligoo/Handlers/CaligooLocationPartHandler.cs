using Laser.Orchard.Caligoo.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Laser.Orchard.Caligoo.Handlers {
    public class CaligooLocationPartHandler : ContentHandler {
        public CaligooLocationPartHandler(IRepository<CaligooLocationPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}