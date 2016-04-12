using Laser.Orchard.UserReactions.Models;
using Laser.Orchard.UserReactions.ViewModels;
using Orchard;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.UserReactions.Services {
    public interface IUserReactionsService : IDependency {
        IQueryable<UserReactionsTypesRecord> GetTypesTable();
        UserReactionsTypes GetTypes();
    }

    public class UserReactionsService : IUserReactionsService {
        private readonly IRepository<UserReactionsTypesRecord> _repoTypes;

        public UserReactionsService(IRepository<UserReactionsTypesRecord> repoTypes) {
            _repoTypes = repoTypes;
        }

        public IQueryable<UserReactionsTypesRecord> GetTypesTable() {
            return _repoTypes.Table.OrderBy(o => o.Priority);
        }

        public UserReactionsTypes GetTypes() {
            var userRT = new UserReactionsTypes();
            userRT.UserReactionsType = GetTypesTable().Select(r => new UserReactionsTypeVM {
                Id = r.Id,
                Priority = r.Priority,
                TypeCssClass = r.TypeCssClass,
                TypeName = r.TypeName, 
                Delete = false
            }).ToList();
            return userRT;
        }


    }
}