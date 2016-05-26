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
        IList<UserReactionsVM> GetTot(UserReactionsPart part);
    }

    //Class definition to user type
    public class UserReactionsService : IUserReactionsService {
        private readonly IRepository<UserReactionsTypesRecord> _repoTypes;
        private readonly IRepository<UserReactionsVM> _repoTot;

        public UserReactionsService(IRepository<UserReactionsTypesRecord> repoTypes, IRepository<UserReactionsVM> repoTot) {
            _repoTypes = repoTypes;
            _repoTot = repoTot;
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



        public IList<UserReactionsVM> GetTot(UserReactionsPart part) {

            IList<UserReactionsVM> viewmodel = new List<UserReactionsVM>();
            viewmodel = part.Reactions.Select(s => new UserReactionsVM {
                Id = s.Id,
                Quantity = s.Quantity,
                TypeName = s.UserReactionsTypesRecord.TypeName,
                TypeId = s.UserReactionsTypesRecord.Id

            }).ToList();

            var ids = viewmodel.Select(s => s.TypeId).ToArray();

            //_userReactionService sono i dati 
            var listType = GetTypesTable().Where(w => !(ids.Contains(w.Id)))
                .Select(x => new UserReactionsVM {
                    Id = 0,
                    Quantity = 0,
                    TypeName = x.TypeName,
                    TypeId = x.Id
                }).ToList();

            viewmodel = viewmodel.Concat(listType).ToList();
            return viewmodel;

        }


    }

}
