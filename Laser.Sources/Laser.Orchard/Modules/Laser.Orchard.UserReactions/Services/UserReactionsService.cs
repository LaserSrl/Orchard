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
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement;
using System.Web.Script.Serialization;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Security.Permissions;
using Orchard.Roles.Services;

namespace Laser.Orchard.UserReactions.Services {

    public interface IUserReactionsService : IDependency {
        IQueryable<UserReactionsTypesRecord> GetTypesTable();
        UserReactionsTypes GetTypes();
        UserReactionsTypes GetTypesTableWithStyles();
        IList<UserReactionsVM> GetTot(UserReactionsPart part);
        IUser CurrentUser();
        UserReactionsVM CalculateTypeClick(int IconType, int CurrentPage);
        UserReactionsVM[] GetSummaryReaction(int CurrentPage);
        UserReactionsPartSettings GetSettingPart(UserReactionsPartSettings Model);
        LocalizedString GetReactionEnumTranslations(ReactionsNames reactionName);
        List<UserReactionsClickRecord> GetListTotalReactions(int Content);
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
        private readonly IRepository<UserReactionsPart> _repoPart;
        private readonly IOrchardServices _orchardServices;
        private readonly IRoleService _roleService;
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

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
                                    IOrchardServices orchardServices,
                                    IRepository<UserReactionsPart> repoPart,
                                    IRoleService roleService) {
            _repoTypes = repoTypes;
            _repoTot = repoTot;
            _authenticationService = authenticationService;
            _repoClick = repoClick;
            _clock = clock;
            _repoUser = repoUser;
            _repoPartRec = repoPartRec;
            _repoSummary = repoSummary;
            _orchardServices = orchardServices;
            _repoPart = repoPart;
            _roleService = roleService;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IQueryable<UserReactionsTypesRecord> GetTypesTable() {

            return _repoTypes.Table.OrderBy(o => o.Priority);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IQueryable<UserReactionsTypesRecord> GetTypesTableFiltered() {

            return _repoTypes.Table.Where(z => z.Activating == true).OrderBy(o => o.Priority);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IQueryable<UserReactionsTypesRecord> GetTypesTableFilteredByTypeReactions(List<UserReactionsSettingTypesSel> typeSettingsReactions) {

            IList<UserReactionsTypesRecord> typeReactionsSelected = _repoTypes.Table.Where(z => z.Activating == true).OrderBy(o => o.Priority).ToList();
            int[] ids = null;
            ids = typeSettingsReactions.Where(s => s.checkReaction == true).Select(s => s.Id).ToArray();
            typeReactionsSelected = typeReactionsSelected.Where(w => (ids.Contains(w.Id))).ToList();

            return typeReactionsSelected.AsQueryable();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="Model"></param>
        /// <returns></returns>
        public UserReactionsPartSettings GetSettingPart(UserReactionsPartSettings Model) {
            UserReactionsPartSettings retval = new UserReactionsPartSettings();
            IQueryable<UserReactionsTypesRecord> repotypesAll = _repoTypes.Table.Where(z => z.Activating == true && z.TypeName != null).OrderBy(o => o.Priority);

            List<UserReactionsSettingTypesSel> partSelectedAll = repotypesAll.Select(r => new UserReactionsSettingTypesSel {
                Id = r.Id,
                nameReaction = r.TypeName,
                checkReaction = false

            }).ToList();

            List<UserReactionsSettingTypesSel> viewmodel;
            List<UserReactionsSettingTypesSel> TypeReactionsPartsModel = new List<UserReactionsSettingTypesSel>();
            TypeReactionsPartsModel = Model.TypeReactionsPartsSelected;

            if (TypeReactionsPartsModel.Count() == 0)
                viewmodel = partSelectedAll;
            else
                viewmodel = Model.TypeReactionsPartsSelected.Except(partSelectedAll).ToList();

            retval.TypeReactionsPartsSelected = viewmodel;
            retval.Filtering = Model.Filtering;
            retval.UserChoiceBehaviour = Model.UserChoiceBehaviour;
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
            userRT.AllowMultipleChoices = reactionSettings.AllowMultipleChoices;
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
        /// ClickTable ordered by date descending.
        /// </summary>
        /// <returns></returns>
        private IQueryable<UserReactionsClickRecord> GetOrderedClickTable() {
            return _repoClick.Table.OrderByDescending(o => o.CreatedUtc);
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
                Activating = r.Activating,
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


        /// <summary>
        /// 
        /// </summary>
        /// <param name="CurrentUser"></param>
        /// <returns></returns>
        public UserReactionsUser ReactionsCurrentUser(IUser CurrentUser) {
            UserReactionsUser reactionsCurrentUser = new UserReactionsUser();
            string userCookie = string.Empty;

            if (CurrentUser != null) {
                reactionsCurrentUser.Id = CurrentUser.Id;
            } else {
                if (HttpContext.Current.Request.Cookies["userCookie"] != null) {
                    userCookie = HttpContext.Current.Request.Cookies["userCookie"].Value.ToString();
                } else {
                    Guid userNameCookie = System.Guid.NewGuid();
                    HttpContext.Current.Response.Cookies.Add(new HttpCookie("userCookie", userNameCookie.ToString()));
                    userCookie = userNameCookie.ToString();

                }
                reactionsCurrentUser.Id = 0;
                reactionsCurrentUser.Guid = userCookie;
            }

            return reactionsCurrentUser;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>       
        public IList<UserReactionsVM> GetTot(UserReactionsPart part) {
            //Part
            IList<UserReactionsVM> viewmodel = new List<UserReactionsVM>();
            //settings type
            List<UserReactionsVM> listType = new List<UserReactionsVM>();

            /////////////////////
            //reaction type settings
            UserReactionsPartSettings settings = part.TypePartDefinition.Settings.GetModel<UserReactionsPartSettings>();
            bool FilterApplied = settings.Filtering;

            List<UserReactionsSettingTypesSel> SettingType = new List<UserReactionsSettingTypesSel>();

            if (part.Settings.Count > 0) {
                SettingType = new JavaScriptSerializer().Deserialize<List<UserReactionsSettingTypesSel>>(part.Settings.Values.ElementAt(1));
            }
            /////////////////////////////////////////////////

            //Reactions type 
            // Prendi i valori delle reactions type
            if (FilterApplied == false) {
                listType = GetTypesTableFiltered()
                .Select(x => new UserReactionsVM {
                    Id = part.Id,
                    Quantity = 0,
                    TypeName = x.TypeName,
                    TypeId = x.Id,
                    CssStyleName = x.TypeCssClass,
                    OrderPriority = x.Priority,
                    Activating = x.Activating,
                }).ToList();
            } else {
                // prendi i valori filtrati
                listType = GetTypesTableFilteredByTypeReactions(SettingType)
                                .Select(x => new UserReactionsVM {
                                    Id = part.Id,
                                    Quantity = 0,
                                    TypeName = x.TypeName,
                                    TypeId = x.Id,
                                    CssStyleName = x.TypeCssClass,
                                    OrderPriority = x.Priority,
                                    Activating = x.Activating
                                }).ToList();

            }

            /////////////////////////////////////////////////////////////////
            //Part type
            viewmodel = part.Reactions.Select(s => new UserReactionsVM {
                Id = s.Id,
                Quantity = s.Quantity,
                TypeName = s.UserReactionsTypesRecord.TypeName,
                TypeId = s.UserReactionsTypesRecord.Id,
                CssStyleName = s.UserReactionsTypesRecord.TypeCssClass,
                OrderPriority = s.UserReactionsTypesRecord.Priority,
                Activating = s.UserReactionsTypesRecord.Activating,
            }).ToList();

            List<UserReactionsVM> retData = new List<UserReactionsVM>();

            foreach (UserReactionsVM itemTypeReactions in listType) {
                UserReactionsVM totItem = itemTypeReactions;
                UserReactionsVM viewModel = viewmodel.FirstOrDefault(z => z.TypeId.Equals(itemTypeReactions.TypeId));

                if (viewModel != null) {
                    totItem.Quantity = viewModel.Quantity;
                }
                retData.Add(totItem);
            }

            return retData;
        }

        private Permission GetPermissionByName(string permission) {
            if (!string.IsNullOrEmpty(permission)) {
                var listpermissions = _roleService.GetInstalledPermissions().Values;
                foreach (IEnumerable<Permission> sad in listpermissions) {
                    foreach (Permission perm in sad) {
                        if (perm.Name == permission) {
                            return perm;
                        }
                    }
                }
            }
            return null;
        }

        /// <param name="CurrentUser"></param>
        /// <param name="IconType"></param>
        /// <param name="CurrentPage"></param>
        /// <returns></returns>
        public UserReactionsVM[] GetSummaryReaction(int CurrentPage) {
            IUser userId = this.CurrentUser(); 
            UserReactionsClickRecord res = new UserReactionsClickRecord();
            string userCookie = string.Empty;
            var part = _orchardServices.ContentManager.Get<UserReactionsPart>(CurrentPage);
            var items = GetTot(part);
            UserReactionsUser reactionsCurrentUser = new UserReactionsUser();
            reactionsCurrentUser = ReactionsCurrentUser(userId);
            List<UserReactionsVM> newSommaryRecord = new List<UserReactionsVM>();
            foreach (UserReactionsVM item in items) {
                UserReactionsVM newItem = new UserReactionsVM();
                newItem = item;

                int IconType = item.TypeId;
                //Verifica che non sia già stato eseguito un click 
                if (reactionsCurrentUser.Id > 0) {
                    res = GetOrderedClickTable().Where(w => w.UserReactionsTypesRecord.Id.Equals(IconType) && w.UserPartRecord.Id.Equals(reactionsCurrentUser.Id) && w.ContentItemRecordId.Equals(CurrentPage)).FirstOrDefault();
                } else {
                    userCookie = reactionsCurrentUser.Guid;
                    res = GetOrderedClickTable().Where(w => w.UserReactionsTypesRecord.Id.Equals(IconType) && w.UserGuid.Equals(userCookie) && w.ContentItemRecordId.Equals(CurrentPage)).FirstOrDefault();
                }
              
                if(res!=null)
                    newItem.Clicked = res.ActionType;
                       
                newSommaryRecord.Add(newItem);
            }

            return newSommaryRecord.ToArray();
        }


        public List<UserReactionsClickRecord> GetListTotalReactions(int Content) 
        {
            var retVal = GetOrderedClickTable().Where(z=>z.ContentItemRecordId==Content).ToList();
            retVal.Reverse();
            return retVal;
        }



        /// <param name="IconType"></param>
        /// <param name="CurrentPage"></param>
        /// <returns></returns>
        public UserReactionsVM CalculateTypeClick(int IconType, int CurrentPage) {
            UserPartRecord userRec = new UserPartRecord();
            UserReactionsTypesRecord reactType = new UserReactionsTypesRecord();
            UserReactionsPartRecord userPart = new UserReactionsPartRecord();
            UserReactionsClickRecord res = new UserReactionsClickRecord();
            UserReactionsClickRecord result = new UserReactionsClickRecord();
            UserReactionsVM retVal = new ViewModels.UserReactionsVM();
            UserReactionsSummaryRecord summaryRecord = new UserReactionsSummaryRecord();
            string userCookie = string.Empty;
            string summaryQty = string.Empty;
            string returnVal = string.Empty;
            int actionType = 1;
            bool previouslyClicked = false;

            //Verifica user
            IUser userId = CurrentUser();
            UserReactionsUser reactionsCurrentUser = new UserReactionsUser();
            reactionsCurrentUser = ReactionsCurrentUser(userId);

            //Verifica che non sia già stato eseguito un click 
            if (reactionsCurrentUser.Id > 0) {
                res = GetOrderedClickTable().Where(w => w.UserReactionsTypesRecord.Id == IconType && w.UserPartRecord.Id == reactionsCurrentUser.Id && w.ContentItemRecordId == CurrentPage).FirstOrDefault();
            } 
            else {
               userCookie = reactionsCurrentUser.Guid;
               res = GetOrderedClickTable().Where(w => w.UserReactionsTypesRecord.Id == IconType && w.UserGuid.Equals(userCookie) && w.ContentItemRecordId == CurrentPage).FirstOrDefault();
            }

            //Se già cliccato quella reaction
            if (res != null) {
                previouslyClicked = true;
                //Già cliccato (Update dati) e ActionType=1   
                if (res.ActionType == 1) {
                    actionType = -1;
                }
            }

            //Salva i dati
            try {
                //Crea nuovo record dati click
                result.CreatedUtc = _clock.UtcNow;
                result.ContentItemRecordId = CurrentPage;
                result.ActionType = actionType;
                reactType = GetTypesTable().Where(w => w.Id.Equals(IconType)).FirstOrDefault();
                result.UserReactionsTypesRecord = reactType;
                if (reactionsCurrentUser.Id > 0) {
                    userRec = _repoUser.Table.Where(w => w.Id.Equals(reactionsCurrentUser.Id)).FirstOrDefault();
                }
                else {
                    userRec = null;
                    result.UserGuid = userCookie;
                }
                result.UserPartRecord = userRec;
                _repoClick.Create(result);

                //Verifica che ci sia già un record cliccato per quell' icona in quel documento
                summaryRecord = _repoSummary.Table.Where(z => z.UserReactionsTypesRecord.Id.Equals(IconType) && z.UserReactionsPartRecord.Id.Equals(CurrentPage)).FirstOrDefault();

                // se 0 record aggiungi il record
                if (summaryRecord == null) {
                    //Create
                    summaryRecord = new UserReactionsSummaryRecord();
                    userPart = _repoPartRec.Table.FirstOrDefault(z => z.Id.Equals(CurrentPage));
                    summaryRecord.Quantity = 1;
                    summaryRecord.UserReactionsTypesRecord = reactType;
                    summaryRecord.UserReactionsPartRecord = userPart;
                    _repoSummary.Create(summaryRecord);

                    if (previouslyClicked) {
                        Logger.Error("UserReactionsService.CalculateTypeClick -> Added missing summary record!");
                    }
                }
                else {
                    // Va in update ed aggiorna il campo Quantity
                    if (actionType == 1) {
                        summaryRecord.Quantity++;
                    }
                    else {
                        summaryRecord.Quantity--;
                    }
                    _repoSummary.Update(summaryRecord);
                }

                retVal.Clicked = 1;
                retVal.Quantity = summaryRecord.Quantity;
                retVal.TypeId = IconType;
                retVal.Id = CurrentPage;
            }
            catch (Exception) {
                retVal.Clicked = 5;
            }

            return retVal;
        }
        public LocalizedString GetReactionEnumTranslations(ReactionsNames reactionName) {
            if (reactionName.Equals(ReactionsNames.angry)) {
                return T("Angry");
            } else if (reactionName.Equals(ReactionsNames.boring)) {
                return T("Boring");
            } else if (reactionName.Equals(ReactionsNames.exahausted)) {
                return T("Exahausted");
            } else if (reactionName.Equals(ReactionsNames.happy)) {
                return T("Happy");
            } else if (reactionName.Equals(ReactionsNames.like)) {
                return T("I Like");
            } else if (reactionName.Equals(ReactionsNames.iwasthere)) {
                return T("I Was There");
            } else if (reactionName.Equals(ReactionsNames.joke)) {
                return T("Joke");
            } else if (reactionName.Equals(ReactionsNames.kiss)) {
                return T("Kiss");
            } else if (reactionName.Equals(ReactionsNames.love)) {
                return T("Love");
            } else if (reactionName.Equals(ReactionsNames.pain)) {
                return T("Pain");
            } else if (reactionName.Equals(ReactionsNames.sad)) {
                return T("Sad");
            } else if (reactionName.Equals(ReactionsNames.shocked)) {
                return T("Shocked");
            } else if (reactionName.Equals(ReactionsNames.silent)) {
                return T("Silent");
            } else {
                return T("None");
            }

        }




    }

}
