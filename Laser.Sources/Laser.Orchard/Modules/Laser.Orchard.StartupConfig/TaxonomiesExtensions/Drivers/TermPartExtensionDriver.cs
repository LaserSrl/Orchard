//using Laser.Orchard.StartupConfig.Services;
//using Orchard;
//using Orchard.ContentManagement;
//using Orchard.ContentManagement.Drivers;
//using Orchard.ContentManagement.Handlers;
//using Orchard.Core.Feeds;
//using Orchard.Environment.Extensions;
//using Orchard.Localization;
//using Orchard.Localization.Models;
//using Orchard.Localization.Services;
//using Orchard.Mvc;
//using Orchard.Settings;
//using Orchard.Taxonomies.Models;
//using Orchard.Taxonomies.Services;
//using Orchard.UI.Navigation;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web.Routing;
//using Laser.Orchard.StartupConfig.Models;
//using Orchard.Core.Common.Models;
//using Orchard.Core.Title.Models;
//using Laser.Orchard.StartupConfig.TaxonomiesExtensions.Services;

//namespace Laser.Orchard.StartupConfig.Drivers {

//    [OrchardFeature("Laser.Orchard.StartupConfig.TaxonomiesExtensions")]
//    [OrchardSuppressDependency("Orchard.Taxonomies.Drivers.TermPartDriver")]
//    public class TermPartExtensionDriver /*: ContentPartDriver<TermPart>*/ {
//        private readonly ITaxonomyService _taxonomyService;
//        private readonly ITaxonomyExtensionsService _taxonomyExtensionsService;
//        private readonly ISiteService _siteService;
//        private readonly IControllerContextAccessor _controllerContextAccessor;
//        private readonly IHttpContextAccessor _httpContextAccessor;
//        private readonly IFeedManager _feedManager;
//        private readonly IContentManager _contentManager;
//        private readonly ILocalizationService _localizationService;
//        private readonly ICultureManager _cultureManager;

//        public TermPartExtensionDriver(
//            ITaxonomyService taxonomyService,
//            ITaxonomyExtensionsService taxonomyExtensionsService,
//            ISiteService siteService,
//            IControllerContextAccessor controllerContextAccessor,
//            IHttpContextAccessor httpContextAccessor,
//            IFeedManager feedManager,
//            IContentManager contentManager,
//            ILocalizationService localizationService,
//            ICultureManager cultureManager) {
//            _taxonomyService = taxonomyService;
//            _taxonomyExtensionsService = taxonomyExtensionsService;
//            _siteService = siteService;
//            _controllerContextAccessor = controllerContextAccessor;
//            _httpContextAccessor = httpContextAccessor;
//            _feedManager = feedManager;
//            _contentManager = contentManager;
//            _localizationService = localizationService;
//            _cultureManager = cultureManager;

//            T = NullLocalizer.Instance;
//        }

//        public Localizer T { get; set; }
//        protected override string Prefix { get { return "Term"; } }

//        protected override DriverResult Display(TermPart part, string displayType, dynamic shapeHelper) {
//            return Combined(
//                ContentShape("Parts_TermPart_Feed", () => {

//                    // generates a link to the RSS feed for this term
//                    _feedManager.Register(part.Name, "rss", new RouteValueDictionary { { "term", part.Id } });
//                    return null;
//                }),
//                ContentShape("Parts_TermPart_Localized", () => {

//                    var pagerParameters = new PagerParameters();
//                    var httpContext = _httpContextAccessor.Current();
//                    if (httpContext != null) {
//                        pagerParameters.Page = Convert.ToInt32(httpContext.Request.QueryString["page"]);
//                    }

//                    var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
//                    var taxonomy = _taxonomyService.GetTaxonomy(part.TaxonomyId);
//                    var totalItemCount = _taxonomyService.GetContentItemsCount(part);

//                    if (taxonomy.As<LocalizationPart>() != null && taxonomy.As<LocalizationPart>().Culture != null) {
//                        _localizationService.SetContentCulture(part.ContentItem, taxonomy.As<LocalizationPart>().Culture.Culture);
//                    }

//                    // asign Taxonomy and Term to the content item shape (Content) in order to provide 
//                    // alternates when those content items are displayed when they are listed on a term
//                    IEnumerable<dynamic> termContentItems;
//                    var baseQuery = _taxonomyService.GetContentItemsQuery(part);
//                    if (taxonomy.ContentItem.As<TaxonomyExtensionPart>().OrderBy == OrderType.PublishedUtc) {
//                        termContentItems = baseQuery
//                                            .Join<CommonPartRecord>()
//                                            .OrderByDescending(x => x.PublishedUtc)
//                                            .Slice(pager.GetStartIndex(), pager.PageSize)
//                           .Select(c => _contentManager.BuildDisplay(c, "Summary").Taxonomy(taxonomy).Term(part));
//                    } else if (taxonomy.ContentItem.As<TaxonomyExtensionPart>().OrderBy == OrderType.Title) {
//                        termContentItems = baseQuery
//                                            .Join<TitlePartRecord>()
//                                            .OrderBy(x => x.Title)
//                                            .Slice(pager.GetStartIndex(), pager.PageSize)
//                           .Select(c => _contentManager.BuildDisplay(c, "Summary").Taxonomy(taxonomy).Term(part));
//                    } else {
//                        termContentItems = _taxonomyService.GetContentItems(part, pager.GetStartIndex(), pager.PageSize)
//                           .Select(c => _contentManager.BuildDisplay(c, "Summary").Taxonomy(taxonomy).Term(part));

//                    }

//                    var list = shapeHelper.List();

//                    list.AddRange(termContentItems);

//                    var pagerShape = shapeHelper.Pager(pager)
//                            .TotalItemCount(totalItemCount)
//                            .Taxonomy(taxonomy)
//                            .Term(part);

//                    return shapeHelper.Parts_TermPart(ContentItems: list, Taxonomy: taxonomy, Pager: pagerShape);
//                }));
//        }

//        protected override DriverResult Editor(TermPart part, dynamic shapeHelper) {
//            return ContentShape("Parts_Taxonomies_Term_Fields",
//                    () => shapeHelper.EditorTemplate(TemplateName: "Parts/Taxonomies.Term.Fields", Model: part, Prefix: Prefix));
//        }

//        protected override DriverResult Editor(TermPart termPart, IUpdateModel updater, dynamic shapeHelper) {
//            if (updater.TryUpdateModel(termPart, Prefix, null, null)) {
//                //Sostituisco gli apici singoli con apostrofi e tolgo gli spazi finali per evitare errori nel javascript che si occupa dell'autocompletamento dei termini
//                termPart.Name = termPart.Name.Replace('\'', '’').Trim();

//                //Se sto creando una traduzione, il termine risulta orfano (TaxonomyId = 0).
//                //Lo associo preventivamente alla tassonomia master, poi nell'handler calcolerò la corretta destinazione.
//                if (termPart.TaxonomyId == 0) {
//                    IContent parentTaxonomy = _taxonomyExtensionsService.GetParentTaxonomy(_contentManager.Get(termPart.Container.Id));
//                    termPart.TaxonomyId = parentTaxonomy.Id;
//                }

//                bool translatingTerm = _controllerContextAccessor.Context.RouteData.Values["action"].ToString() == "Translate" && _controllerContextAccessor.Context.Controller.GetType().FullName == "Orchard.Localization.Controllers.AdminController";

//                //Controllo se il termine esiste già nel container a cui appartiene o dovrà appartenere.
//                string cultureWithDuplicate = _taxonomyExtensionsService.CheckIfDuplicateTerm(termPart, translatingTerm);
//                if (cultureWithDuplicate != null) {
//                    if (termPart.Has<LocalizationPart>()) {
//                        updater.AddModelError("DuplicateTermName", T("The term {0} already exists at this level in the destination taxonomy ({1})", termPart.Name, cultureWithDuplicate));
//                    } else
//                        updater.AddModelError("DuplicateTermName", T("The term {0} already exists at this level", termPart.Name));
//                } else if (translatingTerm && termPart.Has<LocalizationPart>()) {
//                    //Se sto traducendo controllo se la localizzazione del termine corrisponde a quella del padre.

//                    string selectedCulture = System.Web.HttpContext.Current.Request.Form["SelectedCulture"]; //TODO: Trovare un metodo migliore per recuperare l'informazione

//                    //Controllo se il termine ha un termine padre
//                    ContentItem parentTerm = null;
//                    var container = _contentManager.Get(termPart.Container.Id);
//                    if (container.ContentType != "Taxonomy")
//                        parentTerm = container;

//                    if (parentTerm != null) {
//                        if (parentTerm.Has<LocalizationPart>()) {
//                            var termCulture = _cultureManager.GetCultureByName(selectedCulture);
//                            var parentTermCulture = parentTerm.As<LocalizationPart>().Culture;

//                            //Se la cultura è nulla sto creando un nuovo termine, che l'handler dirotterà sulla tassonomia master
//                            if (termCulture != null) {
//                                //Se il padre non è localizzato, va localizzato per primo
//                                if (parentTermCulture == null)
//                                    updater.AddModelError("MissingParentLocalization", T("The parent term is not localized. Please localize it first."));
//                                else {
//                                    //Se invece le due culture sono diverse, cerco se esiste una traduzione del padre con la stessa cultura del contenuto corrente
//                                    if (termCulture != parentTermCulture) {
//                                        //Se non la trovo blocco il salvataggio e segnalo che va creata per prima
//                                        if (_localizationService.GetLocalizedContentItem(parentTerm, termCulture.Culture) == null)
//                                            updater.AddModelError("WrongParentLocalization", T("A localization of the parent term in the specified language does not exist. Please create it first."));
//                                    }
//                                }
//                            }
//                        }
//                    }
//                }
//            }

//            return Editor(termPart, shapeHelper);
//        }

//        protected override void Exporting(TermPart part, ExportContentContext context) {
//            context.Element(part.PartDefinition.Name).SetAttributeValue("Count", part.Count);
//            context.Element(part.PartDefinition.Name).SetAttributeValue("Selectable", part.Selectable);
//            context.Element(part.PartDefinition.Name).SetAttributeValue("Weight", part.Weight);

//            var taxonomy = _contentManager.Get(part.TaxonomyId);
//            var identity = _contentManager.GetItemMetadata(taxonomy).Identity.ToString();
//            context.Element(part.PartDefinition.Name).SetAttributeValue("TaxonomyId", identity);

//            var identityPaths = new List<string>();
//            foreach (var pathPart in part.Path.Split('/')) {
//                if (String.IsNullOrEmpty(pathPart)) {
//                    continue;
//                }

//                var parent = _contentManager.Get(Int32.Parse(pathPart));
//                identityPaths.Add(_contentManager.GetItemMetadata(parent).Identity.ToString());
//            }

//            context.Element(part.PartDefinition.Name).SetAttributeValue("Path", String.Join(",", identityPaths.ToArray()));
//        }

//        protected override void Importing(TermPart part, ImportContentContext context) {
//            part.Count = Int32.Parse(context.Attribute(part.PartDefinition.Name, "Count"));
//            part.Selectable = Boolean.Parse(context.Attribute(part.PartDefinition.Name, "Selectable"));
//            part.Weight = Int32.Parse(context.Attribute(part.PartDefinition.Name, "Weight"));

//            var identity = context.Attribute(part.PartDefinition.Name, "TaxonomyId");
//            var contentItem = context.GetItemFromSession(identity);

//            if (contentItem == null) {
//                throw new OrchardException(T("Unknown taxonomy: {0}", identity));
//            }

//            part.TaxonomyId = contentItem.Id;
//            part.Path = "/";

//            foreach (var identityPath in context.Attribute(part.PartDefinition.Name, "Path").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)) {
//                var pathContentItem = context.GetItemFromSession(identityPath);
//                part.Path += pathContentItem.Id + "/";
//            }
//        }


//    }
//}