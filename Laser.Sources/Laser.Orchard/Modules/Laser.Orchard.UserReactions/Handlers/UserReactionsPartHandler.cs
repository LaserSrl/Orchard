using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Laser.Orchard.UserReactions.Models;


namespace Laser.Orchard.UserReactions.Handlers {
    public class UserReactionsPartHandler : ContentHandler {

        public UserReactionsPartHandler(IRepository<UserReactionsPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }

    }
}