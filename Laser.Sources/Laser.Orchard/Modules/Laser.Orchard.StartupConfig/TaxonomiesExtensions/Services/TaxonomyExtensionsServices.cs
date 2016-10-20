
using Laser.Orchard.StartupConfig.Models;
using Orchard;
using Orchard.Autoroute.Models;
using Orchard.Autoroute.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.Records;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using System;
using System.Linq;

namespace Laser.Orchard.StartupConfig.TaxonomiesExtensions.Services
{
    public interface ITaxonomyExtensionsService : IDependency
    {
        ContentItem GetParentTaxonomy(ContentItem container);
        ContentItem GetParentTerm(ContentItem container);
        void RegenerateAutoroute(ContentItem item);
        void ChangeTermContentType(ContentItem item);
        void InheritParentContentCulture(ContentItem item);
        IContent GetMasterItem(IContent item);
        string CheckIfDuplicateTerm(TermPart term, bool translating);
        void AddLocalizationPartToTerm(TaxonomyPart part);
    }

    
    [OrchardFeature("Laser.Orchard.StartupConfig.TaxonomiesExtensions")]
    public class TaxonomyExtensionsService : ITaxonomyExtensionsService
    {
        private readonly IAutorouteService _autorouteService;
        private readonly IContentManager _contentManager;
        private readonly ILocalizationService _localizationService;
        private readonly IRepository<ContentItemRecord> _contentItemRepository;
        private readonly IRepository<ContentTypeRecord> _contentTypeRepository;
        private readonly ITaxonomyService _taxonomyService;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IOrchardServices _orchardServices;

        public TaxonomyExtensionsService(IAutorouteService autorouteService,
                                         IContentManager contentManager,
                                         ILocalizationService localizationService,
                                         IRepository<ContentItemRecord> contentItemRepository,
                                         IRepository<ContentTypeRecord> contentTypeRepository,
                                         ITaxonomyService taxonomyService,
                                         IContentDefinitionManager contentDefinitionManager,
                                         IOrchardServices orchardServices)
        {
            _autorouteService = autorouteService;
            _contentManager = contentManager;
            _localizationService = localizationService;
            _contentItemRepository = contentItemRepository;
            _contentTypeRepository = contentTypeRepository;
            _taxonomyService = taxonomyService;
            _contentDefinitionManager = contentDefinitionManager;
            _orchardServices = orchardServices;
        }

        /// <summary>
        /// Controlla se il content type rappresentante i termini della tassonomia ha una LocalizationPart e, in base alle impostazioni del sito, se non ce l'ha gliela aggiunge.
        /// </summary>
        /// <param name="part"></param>
        public void AddLocalizationPartToTerm(TaxonomyPart part) {
            var taxonomyExtensionsSiteSettings = _orchardServices.WorkContext.CurrentSite.As<TaxonomyExtensionsSiteSettingsPart>();

            if (taxonomyExtensionsSiteSettings != null) {
                if (taxonomyExtensionsSiteSettings.LocalizeTerms) {
                    ContentTypeDefinition termDefinition = _contentDefinitionManager.GetTypeDefinition(part.TermTypeName);

                    if (termDefinition != null) {
                        if (termDefinition.Parts.Where(x => x.PartDefinition.Name == "LocalizationPart").Count() == 0) {
                            _contentDefinitionManager.AlterTypeDefinition(part.TermTypeName,
                                cfg => cfg
                                    .WithPart("LocalizationPart")
                                );
                        }
                    }
                }
            }
        }

        public ContentItem GetParentTaxonomy(ContentItem container)
        {
            ContentItem parentTaxonomy = container;

            //Poichè ogni tassonomia ha un content type diverso per i termini, do per scontato che qualunque tipo diverso da Taxonomy contenuto nel container sia un termine.
            if (parentTaxonomy.ContentType != "Taxonomy")
            {
                while (parentTaxonomy.ContentType != "Taxonomy")
                    parentTaxonomy = _contentManager.Get(parentTaxonomy.As<TermPart>().Container.Id);
            }

            return parentTaxonomy;
        }

        public ContentItem GetParentTerm(ContentItem container)
        {
            //Poichè ogni tassonomia ha un content type diverso per i termini, do per scontato che qualunque tipo diverso da Taxonomy contenuto nel container sia un termine.
            if (container.ContentType != "Taxonomy")
                return container;
            else
                return null;
        }

        public IContent GetMasterItem(IContent item)
        {
            if (item == null)
                return null;

            var itemLocalization = item.As<LocalizationPart>();
            if (itemLocalization == null)
                return item;
            else
            {
                IContent masterParentTerm = itemLocalization.MasterContentItem;
                if (masterParentTerm == null)
                    masterParentTerm = item;

                return masterParentTerm;
            }
        }

        public void RegenerateAutoroute(ContentItem item)
        {
            if (item.Has<AutoroutePart>())
            {
                _autorouteService.RemoveAliases(item.As<AutoroutePart>());
                item.As<AutoroutePart>().DisplayAlias = _autorouteService.GenerateAlias(item.As<AutoroutePart>());
                _autorouteService.PublishAlias(item.As<AutoroutePart>());
            }
        }

        public void ChangeTermContentType(ContentItem item)
        {
            var record = _contentItemRepository.Get(item.Id);
            if (record != null)
            {
                int contentTypeId = 0;
                var container = item.As<TermPart>().Container;

                //Se è un content type corrispondente a un termine, posso usare il content type del contenitore
                //Altrimenti è una tassonomia e devo recuperare il content type dei suoi termini
                if (container.As<ContentItem>().ContentType != "Taxonomy")
                    contentTypeId = _contentItemRepository.Get(container.Id).ContentType.Id;
                else
                {
                    string termTypeName = container.As<TaxonomyPart>().TermTypeName;
                    ContentTypeRecord termTypeRecord = _contentTypeRepository.Fetch(x => x.Name == termTypeName).FirstOrDefault();

                    if (termTypeRecord != null)
                        contentTypeId = termTypeRecord.Id;
                }

                if (contentTypeId != 0)
                {
                    ContentTypeRecord newContentType = _contentTypeRepository.Get(contentTypeId);
                    record.ContentType = newContentType;
                    _contentItemRepository.Flush();
                }
            }
        }

        public void InheritParentContentCulture(ContentItem item)
        {
            var container = item.As<TermPart>().Container;

            if (container != null)
            {
                string containerCulture = container.As<LocalizationPart>().Culture.Culture;
                if (!String.IsNullOrWhiteSpace(containerCulture))
                    _localizationService.SetContentCulture(item, containerCulture);
            }

            //TODO: Annullare il master content item nel caso in cui imposto la lingua master
        }

        public string CheckIfDuplicateTerm(TermPart term, bool translating)
        {
            TermPart existing = null;
            ContentItemRecord containerRecord = null;
            IContent parentTaxonomy = GetParentTaxonomy(_contentManager.Get(term.Container.Id));

            //Recupero la lingua del termine.
            string termCulture = null;
            if (translating)
                termCulture = System.Web.HttpContext.Current.Request.Form["SelectedCulture"];
            else if (term.Has<LocalizationPart>())
                termCulture = term.As<LocalizationPart>().Culture == null ? null : term.As<LocalizationPart>().Culture.Culture;

            /*
            Se il contenuto non ha cultura devo cercare il termine nella tassonomia corrente se:
            - o il termine non ha una localization part, quindi sono abilitato a crearlo anche nelle tassonomie localizzate
            - o la tassonomia corrente è la master
            In caso contrario se il termine ha una localization part ma cultura nulla è perchè sto usando la funzione di creazione nuovo termine,
            ma se lo sto facendo da una tassonomia localizzata devo dirottare il termine sulla tassonomia master, quindi controllo l'omonimia su di essa.
            */
            if (String.IsNullOrWhiteSpace(termCulture) && (parentTaxonomy == GetMasterItem(parentTaxonomy) || !term.Has<LocalizationPart>()))
            {
                existing = _taxonomyService.GetTermByName(term.TaxonomyId, term.Name);
                containerRecord = term.Container.ContentItem.Record;
            }
            else
            {
                IContent masterTaxonomy = GetMasterItem(_contentManager.Get(term.TaxonomyId));
                IContent localizedTaxonomy = null;

                if (!String.IsNullOrWhiteSpace(termCulture))
                    localizedTaxonomy = _localizationService.GetLocalizedContentItem(masterTaxonomy, termCulture);
                else
                    localizedTaxonomy = masterTaxonomy;

                if (localizedTaxonomy != null)
                {
                    existing = _taxonomyService.GetTermByName(localizedTaxonomy.Id, term.Name);

                    if (existing != null)
                    {
                        IContent localizedContainer = null;
                        if (term.Container.Id != term.TaxonomyId)
                        {
                            IContent masterContainer = GetMasterItem(term.Container);
                            if (!String.IsNullOrWhiteSpace(termCulture))
                                localizedContainer = _localizationService.GetLocalizedContentItem(masterContainer, termCulture);
                            else
                                localizedContainer = masterContainer;
                        }
                        else
                            localizedContainer = localizedTaxonomy;

                        if (localizedContainer != null)
                            containerRecord = localizedContainer.As<IContent>().ContentItem.Record;
                    }
                }
                else
                {
                    existing = _taxonomyService.GetTermByName(term.TaxonomyId, term.Name);
                    containerRecord = term.Container.ContentItem.Record;
                }
            }

            string cultureWithDuplicate = null;
            if (existing != null && existing.Record != term.Record && existing.Container.ContentItem.Record == containerRecord)
            {
                var destinationTaxonomy = GetParentTaxonomy(existing.Container.ContentItem);
                cultureWithDuplicate = _localizationService.GetContentCulture(destinationTaxonomy);
            }

            return cultureWithDuplicate;
        }
    }
}