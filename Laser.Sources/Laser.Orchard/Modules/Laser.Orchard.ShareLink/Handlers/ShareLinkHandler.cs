using Laser.Orchard.ShareLink.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Laser.Orchard.ShareLink.Handlers {

    public class ShareLinkHandler : ContentHandler {

        public ShareLinkHandler(IRepository<ShareLinkPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}