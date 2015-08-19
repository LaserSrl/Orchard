using Laser.Orchard.MessageStore.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Laser.Orchard.MessageStore.Handlers {
    public class MessageStorePartHandler : ContentHandler {
        public MessageStorePartHandler(IRepository<MessageStorePartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}