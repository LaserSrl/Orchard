using Laser.Orchard.DataProtection.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Laser.Orchard.DataProtection.Handlers {
    public class DataContextPartHandler : ContentHandler {
        public DataContextPartHandler(IRepository<DataContextPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}