using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Laser.Orchard.AdvancedSearch.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Core.Common.Models;
using Orchard.Core.Containers.Models;
using Orchard.Core.Contents;
using Orchard.Core.Contents.Settings;
using Orchard.Core.Contents.ViewModels;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Localization.Services;
using Orchard.Logging;
using Orchard.Mvc.Extensions;
using Orchard.Mvc.Html;
using Orchard.Settings;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using Orchard.Utility.Extensions;
using Mvc = Orchard.Mvc;
using Orchard.Localization.Models;
using Orchard.Localization.Records;
using Orchard.Taxonomies.Services;
using Orchard.Taxonomies.Helpers;
using Orchard.Taxonomies.Models;
using Orchard.Users.Models;
using Orchard.Users.Services;
using Laser.Orchard.StartupConfig.Localization;
using Orchard.Projections.Models;
using Orchard.ContentTypes.Services;
using Orchard.Fields.Settings;
using Orchard.Security;
using System.Linq.Expressions;


namespace Laser.Orchard.AdvancedSearch.Controllers {
    public class AdminController : Controller, IUpdateModel {
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentDefinitionService _contentDefinitionService;

        private readonly ITransactionManager _transactionManager;
        private readonly ISiteService _siteService;
        private readonly ICultureManager _cultureManager;
        private readonly IRepository<CultureRecord> _cultureRepo;
        private readonly ITaxonomyService _taxonomyService;
        private readonly IUserService _userService;
        private readonly INotifier _notifier;
        private readonly IDateLocalization _dataLocalization;


        public AdminController(
            IOrchardServices orchardServices,
            IContentManager contentManager,
            IContentDefinitionManager contentDefinitionManager,
            IContentDefinitionService contentDefinitionService,
            ITransactionManager transactionManager,
            ISiteService siteService,
            IShapeFactory shapeFactory,
            ICultureManager cultureManager,
            IRepository<CultureRecord> cultureRepo,
            INotifier notifier,
            IUserService userService,
            IDateLocalization dataLocalization,
            ITaxonomyService taxonomyService) {
            Services = orchardServices;
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
            _contentDefinitionService = contentDefinitionService;
            _transactionManager = transactionManager;
            _siteService = siteService;
            _cultureManager = cultureManager;
            _cultureRepo = cultureRepo;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
            Shape = shapeFactory;
            _taxonomyService = taxonomyService;
            _dataLocalization = dataLocalization;
            _userService = userService;
            _notifier = notifier;

        }

        dynamic Shape { get; set; }
        public IOrchardServices Services { get; private set; }
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public ActionResult List(ListContentsViewModelExtension model, PagerParameters pagerParameters) {
            Pager pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            var versionOptions = VersionOptions.Latest;
            switch (model.Options.ContentsStatus) {
                case ContentsStatus.Published:
                    versionOptions = VersionOptions.Published;
                    break;
                case ContentsStatus.Draft:
                    versionOptions = VersionOptions.Draft;
                    break;
                case ContentsStatus.AllVersions:
                    versionOptions = VersionOptions.AllVersions;
                    break;
                default:
                    versionOptions = VersionOptions.Latest;
                    break;
            }

            var query = _contentManager.Query(versionOptions, GetCreatableTypes(false).Select(ctd => ctd.Name).ToArray());

            if (!string.IsNullOrEmpty(model.TypeName)) {
                var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(model.TypeName);
                if (contentTypeDefinition == null)
                    return HttpNotFound();

                model.TypeDisplayName = !string.IsNullOrWhiteSpace(contentTypeDefinition.DisplayName)
                                            ? contentTypeDefinition.DisplayName
                                            : contentTypeDefinition.Name;
                query = query.ForType(model.TypeName);
            }
            // FILTER QUERIES: START //
            // language query
            //For any language query, remember that Orchard's localization table, as of Orchard 1.8, has an issue where the content
            //created but never translated does not have the default Culture assigned to it.
            if (model.AdvancedOptions.SelectedLanguageId > 0) {
                query = query.Join<LocalizationPartRecord>().Where(x => x.CultureId == model.AdvancedOptions.SelectedLanguageId);
            }


            // terms query
            if (model.AdvancedOptions.SelectedTermId > 0) {
                var termId = model.AdvancedOptions.SelectedTermId;
                query = query.Join<TermsPartRecord>().Where(x => x.Terms.Any(a => a.TermRecord.Id == termId));
            }

            // owner query
            if (    //user cannot see everything by default
                    (
                        !Services.Authorizer.Authorize(AdvancedSearchPermissions.SeesAllContent)
                        || (Services.Authorizer.Authorize(AdvancedSearchPermissions.SeesAllContent) && model.AdvancedOptions.OwnedByMeSeeAll)
                    )&& (//user has either limitation
                        ((Services.Authorizer.Authorize(AdvancedSearchPermissions.MayChooseToSeeOthersContent))
                            && (model.AdvancedOptions.OwnedByMe))
                        || (Services.Authorizer.Authorize(AdvancedSearchPermissions.CanSeeOwnContents) 
                        && ! Services.Authorizer.Authorize(AdvancedSearchPermissions.MayChooseToSeeOthersContent))
                    )
                ) {
                //this user can only see the contents they own
                var lowerName = Services.WorkContext.CurrentUser.UserName.ToLowerInvariant();
                var email = Services.WorkContext.CurrentUser.Email;
                var user = _contentManager.Query<UserPart, UserPartRecord>().Where(u => u.NormalizedUserName == lowerName || u.Email == email).List().FirstOrDefault();
                query = query.Join<CommonPartRecord>().Where(x => x.OwnerId == user.Id);
            } else if (!String.IsNullOrWhiteSpace(model.AdvancedOptions.SelectedOwner)) {
                var lowerName = model.AdvancedOptions.SelectedOwner == null ? "" : model.AdvancedOptions.SelectedOwner.ToLowerInvariant();
                var email = model.AdvancedOptions.SelectedOwner;
                var user = _contentManager.Query<UserPart, UserPartRecord>().Where(u => u.NormalizedUserName == lowerName || u.Email == email).List().FirstOrDefault();
                if (user != null) {
                    query = query.Join<CommonPartRecord>().Where(x => x.OwnerId == user.Id);
                } else {
                    _notifier.Add(NotifyType.Warning, T("No user found. Ownership filter not applied."));
                }
            }

            //date query
            if (model.AdvancedOptions.SelectedFromDate != null) {
                var fromD = _dataLocalization.StringToDatetime(model.AdvancedOptions.SelectedFromDate, "");
                var toD = _dataLocalization.StringToDatetime(model.AdvancedOptions.SelectedToDate, "");

                if (model.AdvancedOptions.DateFilterType == DateFilterOptions.Created)
                    query = query.Join<CommonPartRecord>().Where(x => x.CreatedUtc >= fromD && x.CreatedUtc <= toD);
                else if (model.AdvancedOptions.DateFilterType == DateFilterOptions.Modified)
                    query = query.Join<CommonPartRecord>().Where(x => x.ModifiedUtc >= fromD && x.ModifiedUtc <= toD);
                else if (model.AdvancedOptions.DateFilterType == DateFilterOptions.Published)
                    query = query.Join<CommonPartRecord>().Where(x => x.PublishedUtc >= fromD && x.PublishedUtc <= toD);
            }

            // Has media query
            if (model.AdvancedOptions.HasMedia) {
                var allCt = GetCreatableTypes(false);
                var listFields = new List<string>();
                foreach (var ct in allCt) {
                    var allMediaFld = _contentDefinitionService.GetType(ct.Name).Fields.Where(w =>
                        w._Definition.FieldDefinition.Name == "MediaLibraryPickerField");
                    var allFieldNames = allMediaFld.Select(s => ct.Name + "." + s.Name + ".");
                    listFields.AddRange(allFieldNames);
                }

                query = query.Join<FieldIndexPartRecord>().Where(w => w.StringFieldIndexRecords.Any(
                    w2 => listFields.Contains(w2.PropertyName) && w2.Value != ""
                    ));
            }

            // Extended Status query
            if (!String.IsNullOrWhiteSpace(model.AdvancedOptions.SelectedStatus)) {
                query = query.Join<FieldIndexPartRecord>().Where(w => w.StringFieldIndexRecords.Any(
                    w2 => w2.PropertyName == "PublishExtensionPart.PublishExtensionStatus." && w2.Value == model.AdvancedOptions.SelectedStatus
                    ));
            }
            // FILTER QUERIES: END //


            switch (model.Options.OrderBy) {
                case ContentsOrder.Modified:
                    //query = query.OrderByDescending<ContentPartRecord, int>(ci => ci.ContentItemRecord.Versions.Single(civr => civr.Latest).Id);
                    query = query.OrderByDescending<CommonPartRecord>(cr => cr.ModifiedUtc);
                    break;
                case ContentsOrder.Published:
                    query = query.OrderByDescending<CommonPartRecord>(cr => cr.PublishedUtc);
                    break;
                case ContentsOrder.Created:
                    //query = query.OrderByDescending<ContentPartRecord, int>(ci => ci.Id);
                    query = query.OrderByDescending<CommonPartRecord>(cr => cr.CreatedUtc);
                    break;
            }

            model.Options.SelectedFilter = model.TypeName;
            model.Options.FilterOptions = GetCreatableTypes(false)
                .Select(ctd => new KeyValuePair<string, string>(ctd.Name, ctd.DisplayName))
                .ToList().OrderBy(kvp => kvp.Value);

            // FILTER MODELS: START //
            // language filter model
            model.AdvancedOptions.LanguageOptions = _cultureRepo.Table
                            .Select(ctd => new KeyValuePair<int, string>(ctd.Id, ctd.Culture));
            // taxonomy filter model
            var termList = new List<KeyValuePair<int, string>>();
            foreach (var taxonomy in _taxonomyService.GetTaxonomies()) {
                termList.Add(new KeyValuePair<int, string>(-1, taxonomy.Name));
                foreach (var term in _taxonomyService.GetTerms(taxonomy.Id)) {
                    var gap = new string('-', term.GetLevels());

                    if (gap.Length > 0) {
                        gap += " ";
                    }
                    termList.Add(new KeyValuePair<int, string>(term.Id, gap + term.Name));
                }
            }
            model.AdvancedOptions.TaxonomiesOptions = termList;

            // extended status
            var partDefinition = _contentDefinitionService.GetPart("PublishExtensionPart");
            if (partDefinition != null) {

                var partField = partDefinition.Fields.Where(w => w.Name == "PublishExtensionStatus").SingleOrDefault();
                var settings = partField.Settings.GetModel<EnumerationFieldSettings>().Options;
                string[] options = (!String.IsNullOrWhiteSpace(settings)) ? settings.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.None) : null;
                model.AdvancedOptions.StatusOptions = options.Select(s => new KeyValuePair<string, string>(s, T(s).Text));
            }

            // FILTER MODELS: END //




            var pagerShape = Shape.Pager(pager).TotalItemCount(0);
            var list = Shape.List();

            //the user may not have permission to see anything: in that case, do not execute the query
            if (Services.Authorizer.Authorize(AdvancedSearchPermissions.CanSeeOwnContents)) {
                //if we want only items that do not have a specific translation, we have to do things differently,
                //because the check is done after the query. Hence, for example, we cannot directly page.
                if (model.AdvancedOptions.SelectedUntranslatedLanguageId > 0) {
                    var allCi = query.List();
                    var untranslatedCi = allCi
                        .Where(x =>
                            x.Is<LocalizationPart>() && //some content items may not be translatable
                            (
                                (x.As<LocalizationPart>().Culture != null &&
                                x.As<LocalizationPart>().Culture.Id != model.AdvancedOptions.SelectedUntranslatedLanguageId) ||
                                (x.As<LocalizationPart>().Culture == null) //this is the case where the content was created and never translated to any other culture. 
                                //In that case, in Orchard 1.8, no culture is directly assigned to it, even though the default culture is assumed.
                            ) &&
                            x.As<LocalizationPart>().MasterContentItem == null &&
                            !allCi.Any(y =>
                                y.Is<LocalizationPart>() &&
                                y.As<LocalizationPart>().MasterContentItem == x &&
                                y.As<LocalizationPart>().Culture.Id == model.AdvancedOptions.SelectedUntranslatedLanguageId
                            )
                        );
                    //Paging
                    pagerShape = Shape.Pager(pager).TotalItemCount(untranslatedCi.Count());
                    var pageOfCi = untranslatedCi
                        .Skip(pager.GetStartIndex())
                        .Take((pager.GetStartIndex() + pager.PageSize) > untranslatedCi.Count() ?
                        untranslatedCi.Count() - pager.GetStartIndex() :
                        pager.PageSize)
                        .ToList();
                    list.AddRange(pageOfCi.Select(ci => _contentManager.BuildDisplay(ci, "SummaryAdmin")));
                } else {
                    pagerShape = Shape.Pager(pager).TotalItemCount(query.Count());
                    var pageOfContentItems = query.Slice(pager.GetStartIndex(), pager.PageSize).ToList();
                    list.AddRange(pageOfContentItems.Select(ci => _contentManager.BuildDisplay(ci, "SummaryAdmin")));
                }
            } else {
                Services.Notifier.Error(T("Not authorized to visualize any item."));
            }


            var viewModel = Shape.ViewModel()
                .ContentItems(list)
                .Pager(pagerShape)
                .Options(model.Options)
                .AdvancedOptions(model.AdvancedOptions)
                .TypeDisplayName(model.TypeDisplayName ?? "");

            return View(viewModel);
        }

        private IEnumerable<ContentTypeDefinition> GetCreatableTypes(bool andContainable) {
            return _contentDefinitionManager.ListTypeDefinitions().Where(ctd =>
                Services.Authorizer.Authorize(Permissions.EditContent, _contentManager.New(ctd.Name)) &&
                ctd.Settings.GetModel<ContentTypeSettings>().Creatable &&
                (!andContainable || ctd.Parts.Any(p => p.PartDefinition.Name == "ContainablePart")));
        }

        [HttpPost, ActionName("List")]
        [Mvc.FormValueRequired("submit.Filter")]
        public ActionResult ListFilterPOST(ContentOptions options, AdvancedContentOptions advancedOptions) {
            var routeValues = ControllerContext.RouteData.Values;
            if (options != null) {
                bool seeAll = Services.Authorizer.Authorize(AdvancedSearchPermissions.SeesAllContent);
                bool maySee = Services.Authorizer.Authorize(AdvancedSearchPermissions.MayChooseToSeeOthersContent);
                if ((seeAll && advancedOptions.OwnedByMeSeeAll)
                    || (!seeAll && maySee && advancedOptions.OwnedByMe)) {
                    advancedOptions.SelectedOwner = Services.WorkContext.CurrentUser.UserName;
                }

                routeValues["Options.OrderBy"] = options.OrderBy; //todo: don't hard-code the key
                routeValues["Options.ContentsStatus"] = options.ContentsStatus; //todo: don't hard-code the key
                routeValues["AdvancedOptions.SelectedLanguageId"] = advancedOptions.SelectedLanguageId; //todo: don't hard-code the key
                routeValues["AdvancedOptions.SelectedUntranslatedLanguageId"] = advancedOptions.SelectedUntranslatedLanguageId; //todo: don't hard-code the key
                routeValues["AdvancedOptions.SelectedTermId"] = advancedOptions.SelectedTermId; //todo: don't hard-code the key
                //condition to add the owner to the query string only if we are not going to ignore it anyway
                if (    //user may see everything
                        (seeAll
                        && (!advancedOptions.OwnedByMeSeeAll))
                        ||(  //user does not have limitations
                            (maySee)
                            && (!advancedOptions.OwnedByMe)
                        ) 
                    ) {
                    routeValues["AdvancedOptions.SelectedOwner"] = advancedOptions.SelectedOwner; //todo: don't hard-code the key
                }
                routeValues["AdvancedOptions.SelectedFromDate"] = advancedOptions.SelectedFromDate; //todo: don't hard-code the key
                routeValues["AdvancedOptions.SelectedToDate"] = advancedOptions.SelectedToDate; //todo: don't hard-code the key
                routeValues["AdvancedOptions.DateFilterType"] = advancedOptions.DateFilterType; //todo: don't hard-code the key
                routeValues["AdvancedOptions.HasMedia"] = advancedOptions.HasMedia; //todo: don't hard-code the key
                routeValues["AdvancedOptions.SelectedStatus"] = advancedOptions.SelectedStatus; //todo: don't hard-code the key
                routeValues["AdvancedOptions.OwnedByMe"] = advancedOptions.OwnedByMe; //todo: don't hard-code the key
                routeValues["AdvancedOptions.OwnedByMeSeeAll"] = advancedOptions.OwnedByMeSeeAll; //todo: don't hard-code the key


                if (GetCreatableTypes(false).Any(ctd => string.Equals(ctd.Name, options.SelectedFilter, StringComparison.OrdinalIgnoreCase))) {
                    routeValues["id"] = options.SelectedFilter;
                } else {
                    routeValues.Remove("id");
                }
            }

            return RedirectToAction("List", routeValues);
        }

        [HttpPost, ActionName("List")]
        [Mvc.FormValueRequired("submit.BulkEdit")]
        public ActionResult ListPOST(ContentOptions options, IEnumerable<int> itemIds, string returnUrl) {
            if (itemIds != null) {
                var checkedContentItems = _contentManager.GetMany<ContentItem>(itemIds, VersionOptions.Latest, QueryHints.Empty);
                switch (options.BulkAction) {
                    case ContentsBulkAction.None:
                        break;
                    case ContentsBulkAction.PublishNow:
                        foreach (var item in checkedContentItems) {
                            if (!Services.Authorizer.Authorize(Permissions.PublishContent, item, T("Couldn't publish selected content."))) {
                                _transactionManager.Cancel();
                                return new HttpUnauthorizedResult();
                            }

                            _contentManager.Publish(item);
                        }
                        Services.Notifier.Information(T("Content successfully published."));
                        break;
                    case ContentsBulkAction.Unpublish:
                        foreach (var item in checkedContentItems) {
                            if (!Services.Authorizer.Authorize(Permissions.PublishContent, item, T("Couldn't unpublish selected content."))) {
                                _transactionManager.Cancel();
                                return new HttpUnauthorizedResult();
                            }

                            _contentManager.Unpublish(item);
                        }
                        Services.Notifier.Information(T("Content successfully unpublished."));
                        break;
                    case ContentsBulkAction.Remove:
                        foreach (var item in checkedContentItems) {
                            if (!Services.Authorizer.Authorize(Permissions.DeleteContent, item, T("Couldn't remove selected content."))) {
                                _transactionManager.Cancel();
                                return new HttpUnauthorizedResult();
                            }

                            _contentManager.Remove(item);
                        }
                        Services.Notifier.Information(T("Content successfully removed."));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return this.RedirectLocal(returnUrl, () => RedirectToAction("List"));
        }

        ActionResult CreatableTypeList(int? containerId) {
            var viewModel = Shape.ViewModel(ContentTypes: GetCreatableTypes(containerId.HasValue), ContainerId: containerId);

            return View("CreatableTypeList", viewModel);
        }

        public ActionResult Create(string id, int? containerId) {
            if (string.IsNullOrEmpty(id))
                return CreatableTypeList(containerId);

            var contentItem = _contentManager.New(id);

            if (!Services.Authorizer.Authorize(Permissions.EditContent, contentItem, T("Cannot create content")))
                return new HttpUnauthorizedResult();

            if (containerId.HasValue && contentItem.Is<ContainablePart>()) {
                var common = contentItem.As<CommonPart>();
                if (common != null) {
                    common.Container = _contentManager.Get(containerId.Value);
                }
            }

            var model = _contentManager.BuildEditor(contentItem);
            return View(model);
        }

        [HttpPost, ActionName("Create")]
        [Mvc.FormValueRequired("submit.Save")]
        public ActionResult CreatePOST(string id, string returnUrl) {
            return CreatePOST(id, returnUrl, contentItem => {
                if (!contentItem.Has<IPublishingControlAspect>() && !contentItem.TypeDefinition.Settings.GetModel<ContentTypeSettings>().Draftable)
                    _contentManager.Publish(contentItem);
            });
        }

        [HttpPost, ActionName("Create")]
        [Mvc.FormValueRequired("submit.Publish")]
        public ActionResult CreateAndPublishPOST(string id, string returnUrl) {

            // pass a dummy content to the authorization check to check for "own" variations
            var dummyContent = _contentManager.New(id);

            if (!Services.Authorizer.Authorize(Permissions.PublishContent, dummyContent, T("Couldn't create content")))
                return new HttpUnauthorizedResult();

            return CreatePOST(id, returnUrl, contentItem => _contentManager.Publish(contentItem));
        }

        private ActionResult CreatePOST(string id, string returnUrl, Action<ContentItem> conditionallyPublish) {
            var contentItem = _contentManager.New(id);

            if (!Services.Authorizer.Authorize(Permissions.EditContent, contentItem, T("Couldn't create content")))
                return new HttpUnauthorizedResult();

            _contentManager.Create(contentItem, VersionOptions.Draft);

            var model = _contentManager.UpdateEditor(contentItem, this);

            if (!ModelState.IsValid) {
                _transactionManager.Cancel();
                return View(model);
            }

            conditionallyPublish(contentItem);

            Services.Notifier.Information(string.IsNullOrWhiteSpace(contentItem.TypeDefinition.DisplayName)
                ? T("Your content has been created.")
                : T("Your {0} has been created.", contentItem.TypeDefinition.DisplayName));
            if (!string.IsNullOrEmpty(returnUrl)) {
                return this.RedirectLocal(returnUrl);
            }
            var adminRouteValues = _contentManager.GetItemMetadata(contentItem).AdminRouteValues;
            return RedirectToRoute(adminRouteValues);
        }

        public ActionResult Edit(int id) {
            var contentItem = _contentManager.Get(id, VersionOptions.Latest);

            if (contentItem == null)
                return HttpNotFound();

            if (!Services.Authorizer.Authorize(Permissions.EditContent, contentItem, T("Cannot edit content")))
                return new HttpUnauthorizedResult();

            var model = _contentManager.BuildEditor(contentItem);
            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [Mvc.FormValueRequired("submit.Save")]
        public ActionResult EditPOST(int id, string returnUrl) {
            return EditPOST(id, returnUrl, contentItem => {
                if (!contentItem.Has<IPublishingControlAspect>() && !contentItem.TypeDefinition.Settings.GetModel<ContentTypeSettings>().Draftable)
                    _contentManager.Publish(contentItem);
            });
        }

        [HttpPost, ActionName("Edit")]
        [Mvc.FormValueRequired("submit.Publish")]
        public ActionResult EditAndPublishPOST(int id, string returnUrl) {
            var content = _contentManager.Get(id, VersionOptions.Latest);

            if (content == null)
                return HttpNotFound();

            if (!Services.Authorizer.Authorize(Permissions.PublishContent, content, T("Couldn't publish content")))
                return new HttpUnauthorizedResult();

            return EditPOST(id, returnUrl, contentItem => _contentManager.Publish(contentItem));
        }

        private ActionResult EditPOST(int id, string returnUrl, Action<ContentItem> conditionallyPublish) {
            var contentItem = _contentManager.Get(id, VersionOptions.DraftRequired);

            if (contentItem == null)
                return HttpNotFound();

            if (!Services.Authorizer.Authorize(Permissions.EditContent, contentItem, T("Couldn't edit content")))
                return new HttpUnauthorizedResult();

            string previousRoute = null;
            if (contentItem.Has<IAliasAspect>()
                && !string.IsNullOrWhiteSpace(returnUrl)
                && Request.IsLocalUrl(returnUrl)
                // only if the original returnUrl is the content itself
                && String.Equals(returnUrl, Url.ItemDisplayUrl(contentItem), StringComparison.OrdinalIgnoreCase)
                ) {
                previousRoute = contentItem.As<IAliasAspect>().Path;
            }

            var model = _contentManager.UpdateEditor(contentItem, this);
            if (!ModelState.IsValid) {
                _transactionManager.Cancel();
                return View("Edit", model);
            }

            conditionallyPublish(contentItem);

            if (!string.IsNullOrWhiteSpace(returnUrl)
                && previousRoute != null
                && !String.Equals(contentItem.As<IAliasAspect>().Path, previousRoute, StringComparison.OrdinalIgnoreCase)) {
                returnUrl = Url.ItemDisplayUrl(contentItem);
            }

            Services.Notifier.Information(string.IsNullOrWhiteSpace(contentItem.TypeDefinition.DisplayName)
                ? T("Your content has been saved.")
                : T("Your {0} has been saved.", contentItem.TypeDefinition.DisplayName));

            return this.RedirectLocal(returnUrl, () => RedirectToAction("Edit", new RouteValueDictionary { { "Id", contentItem.Id } }));
        }

        [HttpPost]
        public ActionResult Clone(int id, string returnUrl) {
            var contentItem = _contentManager.GetLatest(id);

            if (contentItem == null)
                return HttpNotFound();

            if (!Services.Authorizer.Authorize(Permissions.EditContent, contentItem, T("Couldn't clone content")))
                return new HttpUnauthorizedResult();

            try {
                Services.ContentManager.Clone(contentItem);
            } catch (InvalidOperationException) {
                Services.Notifier.Warning(T("Could not clone the content item."));
                return this.RedirectLocal(returnUrl, () => RedirectToAction("List"));
            }

            Services.Notifier.Information(T("Successfully cloned. The clone was saved as a draft."));

            return this.RedirectLocal(returnUrl, () => RedirectToAction("List"));
        }

        [HttpPost]
        public ActionResult Remove(int id, string returnUrl) {
            var contentItem = _contentManager.Get(id, VersionOptions.Latest);

            if (!Services.Authorizer.Authorize(Permissions.DeleteContent, contentItem, T("Couldn't remove content")))
                return new HttpUnauthorizedResult();

            if (contentItem != null) {
                _contentManager.Remove(contentItem);
                Services.Notifier.Information(string.IsNullOrWhiteSpace(contentItem.TypeDefinition.DisplayName)
                    ? T("That content has been removed.")
                    : T("That {0} has been removed.", contentItem.TypeDefinition.DisplayName));
            }

            return this.RedirectLocal(returnUrl, () => RedirectToAction("List"));
        }

        [HttpPost]
        public ActionResult Publish(int id, string returnUrl) {
            var contentItem = _contentManager.GetLatest(id);
            if (contentItem == null)
                return HttpNotFound();

            if (!Services.Authorizer.Authorize(Permissions.PublishContent, contentItem, T("Couldn't publish content")))
                return new HttpUnauthorizedResult();

            _contentManager.Publish(contentItem);

            Services.Notifier.Information(string.IsNullOrWhiteSpace(contentItem.TypeDefinition.DisplayName) ? T("That content has been published.") : T("That {0} has been published.", contentItem.TypeDefinition.DisplayName));

            return this.RedirectLocal(returnUrl, () => RedirectToAction("List"));
        }

        [HttpPost]
        public ActionResult Unpublish(int id, string returnUrl) {
            var contentItem = _contentManager.GetLatest(id);
            if (contentItem == null)
                return HttpNotFound();

            if (!Services.Authorizer.Authorize(Permissions.PublishContent, contentItem, T("Couldn't unpublish content")))
                return new HttpUnauthorizedResult();

            _contentManager.Unpublish(contentItem);

            Services.Notifier.Information(string.IsNullOrWhiteSpace(contentItem.TypeDefinition.DisplayName) ? T("That content has been unpublished.") : T("That {0} has been unpublished.", contentItem.TypeDefinition.DisplayName));

            return this.RedirectLocal(returnUrl, () => RedirectToAction("List"));
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }

}