using Orchard.Data;
using Orchard.ContentManagement.Handlers;
using Laser.Orchard.UserReactions.Models;
using Orchard.ContentManagement.MetaData;


namespace Laser.Orchard.UserReactions.Handlers {
    public class UserReactionsPartHandler : ContentHandler {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public UserReactionsPartHandler(IRepository<UserReactionsPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}