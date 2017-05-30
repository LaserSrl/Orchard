using Laser.Orchard.Claims.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Laser.Orchard.Claims.Handlers {
    public class IdentityClaimsPartHandler : ContentHandler {
        public IdentityClaimsPartHandler(IRepository<IdentityClaimsPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}