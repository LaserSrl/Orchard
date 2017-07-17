using Laser.Orchard.ShareLink.Models;
using Laser.Orchard.ShareLink.Servicies;
using Orchard;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Laser.Orchard.ShareLink.Handlers {

    public class ShareLinkHandler : ContentHandler {
        private readonly IOrchardServices _orchardServices;
        private readonly IShareLinkService _sharelinkService;
        public ShareLinkHandler(IRepository<ShareLinkPartRecord> repository, IOrchardServices orchardServices, IShareLinkService sharelinkService) {
            Filters.Add(StorageFilter.For(repository));
            _orchardServices = orchardServices;
            _sharelinkService = sharelinkService;
            OnGetDisplayShape<ShareLinkPart>((context, part) => {
                _sharelinkService.FillPart(part);
            });
        }
    }
}