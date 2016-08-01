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
        UserReactionsVM CalculateTypeClick(int IconType, int CurrentPage);
        ReactionsSummaryVM GetSummaryReaction(int CurrentPage);
        UserReactionsPartSettings GetSettingPart(UserReactionsPartSettings Model);
        LocalizedString GetReactionEnumTranslations(ReactionsNames reactionName);
        List<UserReactionsClickRecord> GetListTotalReactions(int Content);
        bool HasPermission(string contentType);
    }

    //Class definition to user type
    /// <summary>
    /// 
    /// </summary>
    public class UserReactionsService : IUserReactionsService {
        private readonly IRepository<UserReactionsTypesRecord> _repoTypes;
        private readonly IAuthenticationService _authenticationService;
        private readonly IRepository<UserReactionsSummaryRecord> _repoSummary;
        private readonly IRepository<UserReactionsClickRecord> _repoClick;
        private readonly IClock _clock;
        private readonly IRepository<UserPartRecord> _repoUser;
        private readonly IRepository<UserReactionsPartRecord> _repoPartRec;
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
        public UserReactionsService(IRepository<UserReactionsTypesRecord> repoTypes,
                                    IRepository<UserReactionsClickRecord> repoClick,
                                    IAuthenticationService authenticationService,
                                    IClock clock,
                                    IRepository<UserPartRecord> repoUser,
                                    IRepository<UserReactionsPartRecord> repoPartRec,
                                    IRepository<UserReactionsSummaryRecord> repoSummary,
                                    IOrchardServices orchardServices,
                                    IRoleService roleService) {
            _repoTypes = repoTypes;
            _authenticationService = authenticationService;
            _repoClick = repoClick;
            _clock = clock;
            _repoUser = repoUser;
            _repoPartRec = repoPartRec;
            _repoSummary = repoSummary;
            _orchardServices = orchardServices;
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
        /// If there are records with same date, uses Id to order descending.
        /// </summary>
        /// <returns></returns>
        private IQueryable<UserReactionsClickRecord> GetOrderedClickTable() {
            return _repoClick.Table.OrderByDescending(o => o.CreatedUtc).OrderByDescending(o => o.Id);
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
        private IUser CurrentUser() {
            return _authenticationService.GetAuthenticatedUser();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private ReactionsUserIds GetReactionsUserIds(IUser user) {
            ReactionsUserIds ids = new ReactionsUserIds();
            string userCookie = string.Empty;

            if (user != null) {
                ids.Id = user.Id;
            }
            else {
                if (HttpContext.Current.Request.Cookies["userCookie"] != null) {
                    userCookie = HttpContext.Current.Request.Cookies["userCookie"].Value.ToString();
                }
                else {
                    Guid userNameCookie = System.Guid.NewGuid();
                    HttpContext.Current.Response.Cookies.Add(new HttpCookie("userCookie", userNameCookie.ToString()));
                    userCookie = userNameCookie.ToString();
                }
                ids.Id = 0;
                ids.Guid = userCookie;
            }
            return ids;
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
            }
            else {
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
        public ReactionsSummaryVM GetSummaryReaction(int CurrentPage) {
            ReactionsSummaryVM result = new ReactionsSummaryVM();
            IUser userId = this.CurrentUser();
            UserReactionsClickRecord res = new UserReactionsClickRecord();
            string userCookie = string.Empty;
            var part = _orchardServices.ContentManager.Get<UserReactionsPart>(CurrentPage);
            var items = GetTot(part);
            ReactionsUserIds reactionsCurrentUser = new ReactionsUserIds();
            reactionsCurrentUser = GetReactionsUserIds(userId);
            List<UserReactionsVM> newSommaryRecord = new List<UserReactionsVM>();
            foreach (UserReactionsVM item in items) {
                int IconType = item.TypeId;
                //Verifica che non sia già stato eseguito un click 
                if (reactionsCurrentUser.Id > 0) {
                    res = GetOrderedClickTable().Where(w => w.UserReactionsTypesRecord.Id.Equals(IconType) && w.UserPartRecord.Id.Equals(reactionsCurrentUser.Id) && w.ContentItemRecordId.Equals(CurrentPage)).FirstOrDefault();
                }
                else {
                    userCookie = reactionsCurrentUser.Guid;
                    res = GetOrderedClickTable().Where(w => w.UserReactionsTypesRecord.Id.Equals(IconType) && w.UserGuid.Equals(userCookie) && w.ContentItemRecordId.Equals(CurrentPage)).FirstOrDefault();
                }

                if (res != null)
                    item.Clicked = res.ActionType;

                newSommaryRecord.Add(item);
            }

            if (reactionsCurrentUser.Id != 0) {
                result.UserAuthenticated = true;
            }
            if (HasPermission(part.ContentItem.ContentType)) {
                result.UserAuthorized = true;
            }
            result.Reactions = newSommaryRecord.ToArray();
            return result;
        }


        public List<UserReactionsClickRecord> GetListTotalReactions(int Content) {
            var retVal = GetOrderedClickTable().Where(z => z.ContentItemRecordId == Content).ToList();
            retVal.Reverse();
            return retVal;
        }



        /// <param name="iconTypeId"></param>
        /// <param name="pageId"></param>
        /// <returns></returns>
        public UserReactionsVM CalculateTypeClick(int iconTypeId, int pageId) {
            UserReactionsClickRecord previousState = new UserReactionsClickRecord();
            UserReactionsVM retVal = new ViewModels.UserReactionsVM();
            int actionType = 1;
            bool previouslyClicked = false;

            //Verifica user
            IUser currentUser = CurrentUser();
            ReactionsUserIds userIds = GetReactionsUserIds(currentUser);
            var contentItem = _orchardServices.ContentManager.Get(pageId);

            if (HasPermission(contentItem.ContentType)) {
                //Verifica che non sia già stato eseguito un click 
                if (userIds.Id > 0) {
                    previousState = GetOrderedClickTable().Where(w => w.UserReactionsTypesRecord.Id == iconTypeId && w.UserPartRecord.Id == userIds.Id && w.ContentItemRecordId == pageId).FirstOrDefault();
                }
                else {
                    previousState = GetOrderedClickTable().Where(w => w.UserReactionsTypesRecord.Id == iconTypeId && w.UserGuid.Equals(userIds.Guid) && w.ContentItemRecordId == pageId).FirstOrDefault();
                }

                //Se già cliccato quella reaction
                if (previousState != null) {
                    previouslyClicked = true;
                    if (previousState.ActionType == 1) {
                        // se era cliccato allora diventa unclicked
                        actionType = -1;
                    }
                }

                //Salva i dati
                try {
                    UserReactionsTypesRecord reactType = GetTypesTable().Where(w => w.Id.Equals(iconTypeId)).FirstOrDefault();
                    InsertClick(pageId, userIds, actionType, reactType);
                    int qty = UpdateSummary(pageId, userIds, actionType, reactType, previouslyClicked);

                    //gestisce la scelta esclusiva, se richiesto
                    bool isExclusive = IsExclusive(contentItem.ContentType);
                    if (isExclusive && actionType == 1) {
                        // cerca tutti i clicked diversi da quello corrente per lo stesso utente e la stessa pagina
                        var clicked = GetClickedReactions(pageId, userIds);

                        foreach (var reaction in clicked) {
                            // non agisce sulla reaction appena cliccata
                            if (reaction.Id != reactType.Id) {
                                InsertClick(pageId, userIds, -1, reaction);
                                UpdateSummary(pageId, userIds, -1, reaction);
                            }
                        }
                    }

                    retVal.Clicked = 1;
                    retVal.Quantity = qty;
                    retVal.TypeId = iconTypeId;
                    retVal.Id = pageId;
                }
                catch (Exception) {
                    retVal.Clicked = 5;
                }
            }
            else {
                // l'utente non ha le permission
                retVal.Clicked = 1;
                retVal.Quantity = 0;
                retVal.TypeId = iconTypeId;
                retVal.Id = pageId;
            }
            return retVal;
        }
        public LocalizedString GetReactionEnumTranslations(ReactionsNames reactionName) {
            if (reactionName.Equals(ReactionsNames.angry)) {
                return T("Angry");
            }
            else if (reactionName.Equals(ReactionsNames.boring)) {
                return T("Boring");
            }
            else if (reactionName.Equals(ReactionsNames.exahausted)) {
                return T("Exahausted");
            }
            else if (reactionName.Equals(ReactionsNames.happy)) {
                return T("Happy");
            }
            else if (reactionName.Equals(ReactionsNames.like)) {
                return T("I Like");
            }
            else if (reactionName.Equals(ReactionsNames.iwasthere)) {
                return T("I Was There");
            }
            else if (reactionName.Equals(ReactionsNames.joke)) {
                return T("Joke");
            }
            else if (reactionName.Equals(ReactionsNames.kiss)) {
                return T("Kiss");
            }
            else if (reactionName.Equals(ReactionsNames.love)) {
                return T("Love");
            }
            else if (reactionName.Equals(ReactionsNames.pain)) {
                return T("Pain");
            }
            else if (reactionName.Equals(ReactionsNames.sad)) {
                return T("Sad");
            }
            else if (reactionName.Equals(ReactionsNames.shocked)) {
                return T("Shocked");
            }
            else if (reactionName.Equals(ReactionsNames.silent)) {
                return T("Silent");
            }
            else {
                return T("None");
            }
        }

        public bool HasPermission(string contentType) {
            bool result = false;
            Permission permissionToTest = GetPermissionByName("ReactionsFor" + contentType);
            result = _orchardServices.Authorizer.Authorize(permissionToTest);
            return result;
        }
        /// <summary>
        /// Crea nuovo record dati click.
        /// </summary>
        private void InsertClick(int pageId, ReactionsUserIds reactionsUserIds, int actionType, UserReactionsTypesRecord reactType) {
            UserPartRecord userRec = null;
            string guid = null;
            UserReactionsClickRecord clickRecord = new UserReactionsClickRecord();
            clickRecord.CreatedUtc = _clock.UtcNow;
            clickRecord.ContentItemRecordId = pageId;
            clickRecord.ActionType = actionType;
            clickRecord.UserReactionsTypesRecord = reactType;
            if (reactionsUserIds.Id > 0) {
                userRec = _repoUser.Table.Where(w => w.Id.Equals(reactionsUserIds.Id)).FirstOrDefault();
            }
            else {
                guid = reactionsUserIds.Guid;
            }
            clickRecord.UserPartRecord = userRec;
            clickRecord.UserGuid = guid;
            _repoClick.Create(clickRecord);
        }
        private int UpdateSummary(int pageId, ReactionsUserIds reactionsUserIds, int actionType, UserReactionsTypesRecord reactType, bool previouslyClicked = false) {
            UserReactionsSummaryRecord summaryRecord = null;
            UserReactionsPartRecord reactionsPart = null;
            //Verifica che ci sia già un record cliccato per quell' icona in quel documento
            summaryRecord = _repoSummary.Table.Where(z => z.UserReactionsTypesRecord.Id == reactType.Id && z.UserReactionsPartRecord.Id == pageId).FirstOrDefault();

            // se 0 record aggiungi il record
            if (summaryRecord == null) {
                //Create
                summaryRecord = new UserReactionsSummaryRecord();
                reactionsPart = _repoPartRec.Table.FirstOrDefault(z => z.Id.Equals(pageId));
                summaryRecord.Quantity = 1;
                summaryRecord.UserReactionsTypesRecord = reactType;
                summaryRecord.UserReactionsPartRecord = reactionsPart;
                _repoSummary.Create(summaryRecord);

                if (previouslyClicked) {
                    Logger.Error("UserReactionsService.UpdateSummary -> Missing summary record!");
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
            return summaryRecord.Quantity;
        }
        private bool IsExclusive(string contentType) {
            bool result = false;
            var ctypeDefinition = _orchardServices.ContentManager.GetContentTypeDefinitions().Where(x => x.Name == contentType).FirstOrDefault();
            var part = ctypeDefinition.Parts.FirstOrDefault(x => x.PartDefinition.Name == "UserReactionsPart");
            var partSetting = part.Settings.FirstOrDefault(x => x.Key == "UserReactionsPartSettings.UserChoiceBehaviour");
            if (partSetting.Value == "RestrictToSingle") {
                result = true;
            }
            else if (partSetting.Value == "Inherit") {
                var globalSettings = _orchardServices.WorkContext.CurrentSite.As<UserReactionsSettingsPart>();
                if (globalSettings.AllowMultipleChoices == false) {
                    result = true;
                }
            }
            return result;
        }
        private List<UserReactionsTypesRecord> GetClickedReactions(int pageId, ReactionsUserIds userIds) {
            List<UserReactionsTypesRecord> clicked = new List<UserReactionsTypesRecord>();
            List<UserReactionsTypesRecord> unclicked = new List<UserReactionsTypesRecord>();
            var elenco = GetOrderedClickTable().Where(x => (x.UserPartRecord.Id == userIds.Id || x.UserGuid == userIds.Guid) && x.ContentItemRecordId == pageId);
            foreach (var item in elenco) {
                if (clicked.Contains(item.UserReactionsTypesRecord) == false
                    && unclicked.Contains(item.UserReactionsTypesRecord) == false) {
                    if (item.ActionType == 1) {
                        clicked.Add(item.UserReactionsTypesRecord);
                    }
                    else {
                        unclicked.Add(item.UserReactionsTypesRecord);
                    }
                }
            }
            return clicked;
        }
    }
}
