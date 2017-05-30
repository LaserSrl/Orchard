using Laser.Orchard.Claims.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Laser.Orchard.Claims.Handlers {
    public class ItemClaimsPartHandler : ContentHandler {
        public ItemClaimsPartHandler(IRepository<ItemClaimsPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}