//using System.Collections.Generic;
//using System.Linq;
//using System.Web.Routing;
//
//using Orchard.Autoroute.Models;
//using Orchard.Autoroute.Services;
//using Orchard.ContentManagement;
//using Orchard.ContentManagement.Aspects;
//using Orchard.ContentManagement.Handlers;
//using Orchard.Core.Common.Models;
//using Orchard.Environment.Extensions;
//using Orchard.Localization.Models;
//using Orchard.Taxonomies.Models;


//namespace Laser.Orchard.StartupConfig.Handlers {
//    
//    [OrchardFeature("Laser.Orchard.StartupConfig.TaxonomiesExtensions")]
//    public class TermPartHandler : ContentHandler {
//        private readonly IContentManager _contentManager;
//        private readonly IAutorouteService _routeService;

//        public TermPartHandler(RequestContext requestContext, IContentManager contentManager, IAutorouteService routeService) {
//            _contentManager = contentManager;
//            _routeService = routeService;

//            //move terms when created or published
//            OnCreating<TermPart>((context, part) => MigrateTerm(context.ContentItem));
//            OnUpdating<TermPart>((context, part) => MigrateTerm(context.ContentItem));
//            OnPublishing<TermPart>((context, part) => MigrateTerm(context.ContentItem));
//        }

//        //This Method checks the term's culture and it's parent taxonomy's culture and moves it to the correct taxonomy if they aren't equal.
//        private void MigrateTerm(ContentItem item) {
//            if (!item.Has<LocalizationPart>() || !item.Has<TermPart>()) {
//                return;
//            }

//            var taxonomyItem = _contentManager.Get(item.As<TermPart>().TaxonomyId);
//            if (!taxonomyItem.Has<LocalizationPart>() || taxonomyItem.As<LocalizationPart>().Culture == null) {
//                var masterContent = _contentManager.Get(item.As<CommonPart>().Container.Id);
//                if (!masterContent.Has<TermPart>() || masterContent.As<LocalizationPart>().Culture == null) {
//                    return;
//                } else {
//                    taxonomyItem = _contentManager.Get(masterContent.As<TermPart>().TaxonomyId);
//                }
//            }

//            //get our 2 cultures for comparison
//            var taxonomyCulture = taxonomyItem.As<LocalizationPart>().Culture;
//            var termCulture = item.As<LocalizationPart>().Culture;

//            //if the taxonomy is a different culture than the parent taxnomy change the term's parent taxonomy to the right localization...
//            if (termCulture != null && (termCulture.Id != taxonomyCulture.Id)) {
//                //Get the id of the current taxonomy
//                var taxonomyids = new HashSet<int> { taxonomyItem.As<TaxonomyPart>().Record.ContentItemRecord.Id };

//                //Add master taxonomy id if current taxonomy is a translation (child of the master taxonomy)
//                _contentManager.Query("Taxonomy")
//                               .Join<LocalizationPartRecord>()
//                               .Where(x => x.MasterContentItemId == taxonomyItem.As<TaxonomyPart>().ContentItem.Id)
//                               .List().ToList().ForEach(x => taxonomyids.Add(x.Id));

//                //and look in all master taxonomy's translations
//                if (taxonomyItem.As<LocalizationPart>() != null && taxonomyItem.As<LocalizationPart>().MasterContentItem != null) {
//                    _contentManager.Query("Taxonomy").Join<CommonPartRecord>().Where(x => x.Id == taxonomyItem.As<LocalizationPart>().MasterContentItem.Id).List().ToList().ForEach(x => taxonomyids.Add(x.Id));
//                }

//                foreach (var taxonomyid in taxonomyids) {
//                    var thisTaxonomy = _contentManager.Get(taxonomyid);

//                    //find this poor, orphaned, term a new daddy
//                    if (thisTaxonomy.Has<LocalizationPart>() && thisTaxonomy.As<LocalizationPart>().Culture.Id == termCulture.Id) {
//                        item.As<TermPart>().TaxonomyId = thisTaxonomy.Id;
//                        if (item.Has<AutoroutePart>()) {
//                            _routeService.RemoveAliases(item.As<AutoroutePart>());
//                            item.As<AutoroutePart>().DisplayAlias = _routeService.GenerateAlias(item.As<AutoroutePart>());
//                            _routeService.PublishAlias(item.As<AutoroutePart>());
//                        }
//                        return;
//                    }
//                }
//            }
//        }
//    }
//}