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
using Laser.Orchard.UserReactions.Services;
using Laser.Orchard.UserReactions.ViewModels;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement;
using System.Web.Script.Serialization;


namespace Laser.Orchard.UserReactions.Services {

    public interface IUserReactionsService : IDependency {
        IQueryable<UserReactionsTypesRecord> GetTypesTable();
        UserReactionsTypes GetTypes();
        UserReactionsTypes GetTypesTableWithStyles();
        IList<UserReactionsVM> GetTot(UserReactionsPart part, bool filter);
        IUser CurrentUser();
        int CalculateTypeClick(IUser CurrentUser, int IconType, int CurrentPage);
        UserReactionsPartSettings GetSettingPart(UserReactionsPartSettings Model);

    }

    
    //Class definition to user type
    /// <summary>
    /// 
    /// </summary>
    public class UserReactionsService : IUserReactionsService {
        private readonly IRepository<UserReactionsTypesRecord> _repoTypes;
        private readonly IRepository<UserReactionsVM> _repoTot;
        private readonly IAuthenticationService _authenticationService;
        private readonly IRepository<UserReactionsSummaryRecord> _repoSummary;
        private readonly IRepository<UserReactionsClickRecord> _repoClick;
        private readonly IClock _clock;
        private readonly IRepository<UserPartRecord> _repoUser;
        private readonly IRepository<UserReactionsPartRecord> _repoPartRec;
        private readonly IOrchardServices _orchardServices;
        //private readonly IRepository<ContentItemRecord> _repoContent;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="repoTypes"></param>
        /// <param name="repoTot"></param>
        /// <param name="repoClick"></param>
        /// <param name="authenticationService"></param>
        /// <param name="clock"></param>
        /// <param name="repoUser"></param>
        /// <param name="repoPartRec"></param>
        /// <param name="repoSummary"></param>
        public UserReactionsService(IRepository<UserReactionsTypesRecord> repoTypes, IRepository<UserReactionsVM> repoTot, 
                                    IRepository<UserReactionsClickRecord> repoClick,
                                    IAuthenticationService authenticationService, 
                                    IClock clock,
                                    IRepository<UserPartRecord> repoUser,
                                    IRepository<UserReactionsPartRecord> repoPartRec,
                                    IRepository<UserReactionsSummaryRecord> repoSummary,
                                    IOrchardServices orchardServices
) 
        {
            _repoTypes = repoTypes;
            _repoTot = repoTot;
            _authenticationService = authenticationService;
            _repoClick = repoClick;
            _clock = clock;
            _repoUser = repoUser;
            _repoPartRec = repoPartRec;
            _repoSummary = repoSummary;
            _orchardServices = orchardServices;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IQueryable<UserReactionsTypesRecord> GetTypesTable() {
            
            return _repoTypes.Table.OrderBy(o => o.Priority);
        }

        public IQueryable<UserReactionsTypesRecord> GetTypesTableFiltered() {

            return _repoTypes.Table.Where(z=>z.Activating==true).OrderBy(o => o.Priority);
        }



        public UserReactionsPartSettings GetSettingPart(UserReactionsPartSettings Model) 
        {
            UserReactionsPartSettings retval = new UserReactionsPartSettings();
            IQueryable<UserReactionsTypesRecord> repotypesAll = _repoTypes.Table.Where(z => z.Activating == true && z.TypeName != null).OrderBy(o => o.Priority);
            
            List<UserReactionsSettingTypesSel> partSelectedAll = repotypesAll.Select(r=> new UserReactionsSettingTypesSel 
            {
                 Id = r.Id,
                 nameReaction =r.TypeName,
                 checkReaction = false
                 
            }).ToList();


            List<UserReactionsSettingTypesSel> viewmodel;
            List<UserReactionsSettingTypesSel> TypeReactionsPartsModel = new List<UserReactionsSettingTypesSel>();
            TypeReactionsPartsModel = Model.TypeReactionsPartsSelected;
      
      
            if (TypeReactionsPartsModel.Count()==0)
                viewmodel = partSelectedAll;
            else                
                viewmodel = Model.TypeReactionsPartsSelected.Except(partSelectedAll).ToList();

            retval.TypeReactionsPartsSelected = viewmodel;
            retval.Filtering = Model.Filtering;
            return retval;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public UserReactionsTypes GetTypesTableWithStyles() {

            var reactionSettings = _orchardServices.WorkContext.CurrentSite.As<UserReactionsSettingsPart>();
            //UserReactionsSettingsPart fileCssName = new UserReactionsSettingsPart();

       
            var userRT = new UserReactionsTypes();
            userRT.CssName = reactionSettings.StyleFileNameProvider;
            
            userRT.UserReactionsType = GetTypesTable().Select(r => new UserReactionsTypeVM {
                Id = r.Id,
                Priority = r.Priority,
                TypeCssClass = r.TypeCssClass,
                TypeName = r.TypeName,
                Activating = r.Activating,
                Delete = false
            }).ToList();
            return userRT;
        }

       

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
         public IQueryable<UserReactionsClickRecord> GetClickTable() {
             return _repoClick.Table.OrderBy(o => o.Id);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public UserReactionsTypes GetTypes() {

            var userRT = new UserReactionsTypes();

            userRT.UserReactionsType = GetTypesTable().Select(r => new UserReactionsTypeVM {
                Id = r.Id,
                Priority = r.Priority,
                TypeCssClass = r.TypeCssClass,
                TypeName = r.TypeName,
                Activating=r.Activating,
                Delete = false
            }).ToList();
            return userRT;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IUser CurrentUser() {
           return _authenticationService.GetAuthenticatedUser();
        }
                   

        /// <param name="part"></param>
        /// <returns></returns>
        public IList<UserReactionsVM> GetTot(UserReactionsPart part, bool filter) {
           
            var reactionSettings = _orchardServices.WorkContext.CurrentSite.As<UserReactionsSettingsPart>();           
            IList<UserReactionsVM> viewmodel = new List<UserReactionsVM>();
            List<UserReactionsVM> listType = new List<UserReactionsVM>();
            int[] ids = null;

            viewmodel = part.Reactions.Select(s => new UserReactionsVM {
                Id = s.Id,
                Quantity = s.Quantity,
                TypeName = s.UserReactionsTypesRecord.TypeName,
                TypeId = s.UserReactionsTypesRecord.Id,
                CssStyleName = s.UserReactionsTypesRecord.TypeCssClass, 
                OrderPriority = s.UserReactionsTypesRecord.Priority,
                Activating = s.UserReactionsTypesRecord.Activating,
                CssName = reactionSettings.StyleFileNameProvider 
            }).ToList();

            if (filter == false) {
                
                ids = viewmodel.Select(s => s.TypeId).ToArray();

                listType = GetTypesTableFiltered().Where(w => !(ids.Contains(w.Id)))
                .Select(x => new UserReactionsVM {
                    Id = 0,
                    Quantity = 0,
                    TypeName = x.TypeName,
                    TypeId = x.Id,
                    CssStyleName = x.TypeCssClass,
                    OrderPriority = x.Priority,
                    Activating = x.Activating,
                    CssName = reactionSettings.StyleFileNameProvider
                }).ToList();

                viewmodel = viewmodel.Concat(listType).Where(r => r.Activating == true).OrderBy(z => z.OrderPriority).ToList();
            } 
            else 
            {
                List<UserReactionsSettingTypesSel> SettingType = new List<UserReactionsSettingTypesSel>();

                if (part.Settings.Count > 0) {
                    SettingType = new JavaScriptSerializer().Deserialize<List<UserReactionsSettingTypesSel>>(part.Settings.Values.ElementAt(1));
                    ids = SettingType.Where(z => z.checkReaction == true).Select(x => x.Id).ToArray();

                    listType = GetTypesTableFiltered().Where(w => (ids.Contains(w.Id)))
                                    .Select(x => new UserReactionsVM {
                                    Id = 0,
                                    Quantity = 0,
                                    TypeName = x.TypeName,
                                    TypeId = x.Id,
                                    CssStyleName = x.TypeCssClass,
                                    OrderPriority = x.Priority,
                                    Activating = x.Activating,
                                    CssName = reactionSettings.StyleFileNameProvider
                                }).ToList();

                    viewmodel = viewmodel.Except(listType).OrderBy(z => z.OrderPriority).ToList();
                }   
            }
          
            return viewmodel;
        }



        /// <param name="CurrentUser"></param>
        /// <param name="IconType"></param>
        /// <param name="CurrentPage"></param>
        /// <returns></returns>
        public int CalculateTypeClick(IUser CurrentUser, int IconType, int CurrentPage) {
            int returnVal = 0;
            UserPartRecord userRec = new UserPartRecord();
            UserReactionsTypesRecord reactType = new UserReactionsTypesRecord();
            UserReactionsPartRecord userPart = new UserReactionsPartRecord();
            UserReactionsClickRecord res = new UserReactionsClickRecord();
            string userCookie = string.Empty;

            //Verifica che non sia già stato eseguito un click 
            if (CurrentUser != null) 
            {
                res = GetClickTable().Where(w => w.UserReactionsTypesRecord.Id.Equals(IconType) && w.UserPartRecord.Id.Equals(CurrentUser.Id) && w.ActionType.Equals(1)).FirstOrDefault();
            } 
            else 
            {                
                //Leggi il cookie se vuoto crea il guid altrimenti prendi il valore del cookie
                if (HttpContext.Current.Request.Cookies["userCookie"] != null) 
                {
                    userCookie = HttpContext.Current.Request.Cookies["userCookie"].Value.ToString();                    
                } 
                else
                {
                    Guid userNameCookie = System.Guid.NewGuid();
                    HttpContext.Current.Response.Cookies.Add(new HttpCookie("userCookie", userNameCookie.ToString()));
                    userCookie = userNameCookie.ToString();                                                                               
                   
                }

                res = GetClickTable().Where(w => w.UserReactionsTypesRecord.Id.Equals(IconType) && w.UserGuid.Equals(userCookie) && w.ActionType.Equals(1)).FirstOrDefault();
            }

            UserReactionsClickRecord result = new UserReactionsClickRecord();



            //Se già cliccato quella reaction
            if (res != null) 
            {
                //Già cliccato (Update dati)   
                res.ActionType = -1;
                _repoClick.Update(res);

                UserReactionsSummaryRecord sommaryRecord = new UserReactionsSummaryRecord();

                //Verifica che ci sia già un record cliccato per quell' icona in quel documento
                sommaryRecord = _repoSummary.Table.Where(z => z.UserReactionsTypesRecord.Id.Equals(IconType) && z.UserReactionsPartRecord.Id.Equals(CurrentPage)).FirstOrDefault();

                if (sommaryRecord.Quantity > 0) {
                    sommaryRecord.Quantity = sommaryRecord.Quantity - 1;
                    _repoSummary.Update(sommaryRecord);
                }

                if(CurrentUser==null)
                    returnVal = 0;
                else
                    returnVal = -1;


                return returnVal;
            } 
            else 
            {
                //Crea record dati click mai eseguito su quella emoticon                                 
                result.CreatedUtc = _clock.UtcNow;

                result.ContentItemRecordId = CurrentPage;
                result.ActionType = 1;

                reactType = GetTypesTable().Where(w => w.Id.Equals(IconType)).FirstOrDefault();
                result.UserReactionsTypesRecord = reactType;

                if (CurrentUser != null) 
                {
                    userRec = _repoUser.Table.Where(w => w.Id.Equals(CurrentUser.Id)).FirstOrDefault();
                } 
                else 
                {                        
                    userRec = null;
                    result.UserGuid = userCookie;
                }
                       
           }


            result.UserPartRecord = userRec;

            //Salva i dati
            try {
                _repoClick.Create(result);

                //Aggiungi il click nella tabella summary
                UserReactionsSummaryRecord sommaryRecord = new UserReactionsSummaryRecord();

                //Verifica che ci sia già un record cliccato per quell' icona in quel documento
                sommaryRecord = _repoSummary.Table.Where(z => z.UserReactionsTypesRecord.Id.Equals(IconType) && z.UserReactionsPartRecord.Id.Equals(CurrentPage)).FirstOrDefault();

                // se 0 record aggiungi il record
                if (sommaryRecord == null) {
                    //Create
                    UserReactionsSummaryRecord sommaryRec = new UserReactionsSummaryRecord();
                    userPart = _repoPartRec.Table.FirstOrDefault(z => z.Id.Equals(CurrentPage));

                    sommaryRec.Quantity = 1;
                    sommaryRec.UserReactionsTypesRecord = reactType;
                    sommaryRec.UserReactionsPartRecord = userPart;
                    _repoSummary.Create(sommaryRec);
                }
                    // Vai in update ed aggiorna il campo Quantity
                else {
                    sommaryRecord.Quantity = sommaryRecord.Quantity + 1;
                    _repoSummary.Update(sommaryRecord);
                }

                if (CurrentUser == null)
                    returnVal = 2;
                else
                    returnVal = 1;

            } catch (Exception) {
                returnVal = 5;
            }            

            return returnVal;
        }




    }

}
