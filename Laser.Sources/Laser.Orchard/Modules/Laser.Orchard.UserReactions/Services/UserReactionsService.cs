using Laser.Orchard.UserReactions.Models;
using Laser.Orchard.UserReactions.ViewModels;
using Orchard;
using Orchard.Data;
using Orchard.Security;
using Orchard.Services;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.UserReactions.Services {

    public interface IUserReactionsService : IDependency {
        IQueryable<UserReactionsTypesRecord> GetTypesTable();
        UserReactionsTypes GetTypes();
        IList<UserReactionsVM> GetTot(UserReactionsPart part);
        IUser CurrentUser();
        int CalculateTypeClick(IUser CurrentUser, int IconType, int CurrentPage);

    }

    //Class definition to user type
    public class UserReactionsService : IUserReactionsService {
        private readonly IRepository<UserReactionsTypesRecord> _repoTypes;
        private readonly IRepository<UserReactionsVM> _repoTot;
        private readonly IAuthenticationService _authenticationService;
        private readonly IRepository<UserReactionsClickRecord> _repositoryReactionClick;
        private readonly IRepository<UserReactionsClickRecord> _repoClick;
        private readonly IClock _clock;
        private readonly IRepository<UserPartRecord> _repoUser;

        public UserReactionsService(IRepository<UserReactionsTypesRecord> repoTypes, IRepository<UserReactionsVM> repoTot, 
                                    IRepository<UserReactionsClickRecord> repoClick,
                                    IAuthenticationService authenticationService, 
                                    IClock clock,
                                    IRepository<UserPartRecord> repoUser) 
        {
            _repoTypes = repoTypes;
            _repoTot = repoTot;
            _authenticationService = authenticationService;
            _repoClick = repoClick;
            _clock = clock;
            _repoUser = repoUser;
        }

        public IQueryable<UserReactionsTypesRecord> GetTypesTable() {
            return _repoTypes.Table.OrderBy(o => o.Priority);
        }

         public IQueryable<UserReactionsClickRecord> GetClickTable() {
             return _repoClick.Table.OrderBy(o => o.Id);
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


        public IUser CurrentUser() {
           return _authenticationService.GetAuthenticatedUser();
        }
                   

        /// <summary>
        /// 
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
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


        /// <summary>
        /// 
        /// </summary>
        /// <param name="CurrentUser"></param>
        /// <param name="IconType"></param>
        /// <param name="CurrentPage"></param>
        /// <returns></returns>
        public int CalculateTypeClick(IUser CurrentUser, int IconType, int CurrentPage) {
            int returnVal = 0;

            //Verifica che non sia già stato eseguito un click
            UserReactionsClickRecord res= GetClickTable().Where(w => w.UserReactionsTypesRecord.Id.Equals(IconType) && w.UserPartRecord.Id.Equals(CurrentUser.Id)).FirstOrDefault();
            
            if (res != null) 
            {
                //Già cliccato (Update dati)   
                res.ActionType = -1;
                _repoClick.Update(res);
                returnVal = - 1;
            }               
            else 
            {
                //Crea record dati click mai eseguito su quella emoticon             
                UserReactionsClickRecord result = new UserReactionsClickRecord();
                result.CreatedUtc = _clock.UtcNow;

                result.ContentItemRecordId = CurrentPage;
                result.ActionType = 1;

                UserReactionsTypesRecord reactType = GetTypesTable().Where(w => w.Id.Equals(IconType)).FirstOrDefault();
                result.UserReactionsTypesRecord = reactType;

                UserPartRecord userRec= _repoUser.Table.Where(w => w.Id.Equals(CurrentUser.Id)).FirstOrDefault();
                result.UserPartRecord = userRec;  

                //Salva i dati
                try
                {
                    _repoClick.Create(result);
                    returnVal = 1;  
              
                } catch (Exception) {
                    returnVal = 0;  
                }
            }

            return returnVal;
        }


    }

}
