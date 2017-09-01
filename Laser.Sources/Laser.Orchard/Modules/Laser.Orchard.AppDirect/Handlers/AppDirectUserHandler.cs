using Laser.Orchard.AppDirect.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Laser.Orchard.AppDirect.Handlers {
    public class AppDirectUserHandler : ContentHandler {
        public AppDirectUserHandler(IRepository<AppDirectUserPartRecord> repositoryAppDirectUserPartRecord) {
            Filters.Add(StorageFilter.For(repositoryAppDirectUserPartRecord));
        }
    }
}
