using Orchard.Data;
using Orchard.ContentManagement.Handlers;
using Laser.Orchard.UserReactions.Models;
using Laser.Orchard.UserReactions.Services;
using Laser.Orchard.UserReactions.Drivers;

namespace Laser.Orchard.UserReactions.Handlers {
    public class UserReactionsListPartHandler: ContentHandler {
        public UserReactionsListPartHandler(IRepository<UserReactionsListPartRecord> repository) 
        {
           Filters.Add(new ActivatingFilter<UserReactionsListPart>("Site"));
           Filters.Add(StorageFilter.For(repository));
            
        }
    }
}