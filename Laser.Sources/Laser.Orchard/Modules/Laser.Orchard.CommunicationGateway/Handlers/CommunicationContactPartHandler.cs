using Laser.Orchard.CommunicationGateway.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Localization;

namespace Laser.Orchard.CommunicationGateway.Handlers {

    public class CommunicationContactPartHandler : ContentHandler {
        public Localizer T { get; set; }

        public CommunicationContactPartHandler(IRepository<CommunicationContactPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
            T = NullLocalizer.Instance;
        }
    }
}