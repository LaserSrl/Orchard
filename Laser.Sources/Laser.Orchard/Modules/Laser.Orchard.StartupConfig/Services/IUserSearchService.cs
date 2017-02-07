using Orchard;
using Orchard.Security;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.Services {
    public interface IUserSearchService : IDependency {
        IEnumerable<IUser> SearchByNameOrEmail(string searchText);
    }
    public class UserSearchService : IUserSearchService {
        private readonly IOrchardServices _orchardServices;
        public UserSearchService(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
        }
        public IEnumerable<IUser> SearchByNameOrEmail(string searchText) {
            var query = _orchardServices.ContentManager.Query().ForPart<UserPart>()
                .Where<UserPartRecord>(u => u.UserName.Contains(searchText) || u.Email.Contains(searchText));
            return (IEnumerable<IUser>)(query.List());
        }
    }
}