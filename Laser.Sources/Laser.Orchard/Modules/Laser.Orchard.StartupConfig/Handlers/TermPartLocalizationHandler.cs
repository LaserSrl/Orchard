
using Laser.Orchard.StartupConfig.Models;
using Laser.Orchard.StartupConfig.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using Orchard.Taxonomies.Models;
using Orchard.UI.Notify;
using System.Linq;

namespace Laser.Orchard.StartupConfig.Handlers
{
    
    [OrchardFeature("Laser.Orchard.StartupConfig.TaxonomiesExtensions")]
    public class TermPartLocalizationHandler : ContentHandler
    {
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IControllerContextAccessor _controllerContextAccessor;
        private readonly ILocalizationService _localizationService;
        private readonly INotifier _notifier;
        private readonly IOrchardServices _orchardServices;
        private readonly ITaxonomyExtensionsService _taxonomyExtensionsServices;

        public Localizer T { get; set; }

        public TermPartLocalizationHandler(IContentManager contentManager,
                                           IContentDefinitionManager contentDefinitionManager,
                                           IControllerContextAccessor controllerContextAccessor,
                                           ILocalizationService localizationService,
                                           INotifier notifier,
                                           IOrchardServices orchardServices,
                                           ITaxonomyExtensionsService taxonomyExtensionServices)
        {
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
            _controllerContextAccessor = controllerContextAccessor;
            _localizationService = localizationService;
            _notifier = notifier;
            _orchardServices = orchardServices;
            _taxonomyExtensionsServices = taxonomyExtensionServices;

            T = NullLocalizer.Instance;

            OnPublished<TaxonomyPart>((context, part) => AddLocalizationPartToTerm(part));
            OnPublishing<TermPart>((context, part) => CheckTermLocalization(context.ContentItem));
        }

        //Controlla se il content type rappresentante i termini della tassonomia ha una LocalizationPart e, in base alle impostazioni del sito, se non ce l'ha gliela aggiunge
        private void AddLocalizationPartToTerm(TaxonomyPart part)
        {
            var taxonomyExtensionsSiteSettings = _orchardServices.WorkContext.CurrentSite.As<TaxonomyExtensionsSiteSettingsPart>();

            if (taxonomyExtensionsSiteSettings != null)
            {
                if (taxonomyExtensionsSiteSettings.LocalizeTerms)
                {
                    ContentTypeDefinition termDefinition = _contentDefinitionManager.GetTypeDefinition(part.TermTypeName);

                    if (termDefinition != null)
                    {
                        if (termDefinition.Parts.Where(x => x.PartDefinition.Name == "LocalizationPart").Count() == 0)
                        {
                            _contentDefinitionManager.AlterTypeDefinition(part.TermTypeName,
                                cfg => cfg
                                    .WithPart("LocalizationPart")
                                );
                        }
                    }
                }
            }
        }

        private void CheckTermLocalization(ContentItem termItem)
        {
            if (!termItem.Has<LocalizationPart>()) return;

            //Recupero la tassonomia di appartenenza e il corrispondente termine padre.
            var container = _contentManager.Get(termItem.As<TermPart>().Container.Id);
            ContentItem parentTerm = _taxonomyExtensionsServices.GetParentTerm(container);
            ContentItem parentTaxonomy = _taxonomyExtensionsServices.GetParentTaxonomy(container);

            bool movingTerm = _controllerContextAccessor.Context.RouteData.Values["action"].ToString() == "MoveTerm" && _controllerContextAccessor.Context.Controller.GetType().FullName == "Orchard.Taxonomies.Controllers.TermAdminController";

            //Se sto spostando un termine lo permetto sempre senza fare nulla.
            if (!movingTerm)
            {
                //Se la cultura è nulla sto creando un nuovo termine. I nuovi termini vanno associati sempre alla tassonomia master.
                if (termItem.As<LocalizationPart>().Culture == null)
                {
                    IContent masterTaxonomy = parentTaxonomy.As<LocalizationPart>().MasterContentItem;
                    IContent masterParentTerm = null;
                    if (parentTerm != null)
                        masterParentTerm = parentTerm.As<LocalizationPart>().MasterContentItem;

                    //Se masterTaxonomy e masterParentTerm sono nulli, sono nella tassonomia master e quindi non devo fare nulla.
                    //Altrimenti se ha un padre lo attacco al master del padre, mentre in caso contrario alla tassonomia master.
                    if (masterParentTerm != null)
                    {
                        termItem.As<TermPart>().Container = masterParentTerm;
                        termItem.As<TermPart>().Path = masterParentTerm.As<TermPart>().FullPath + "/";
                        termItem.As<TermPart>().TaxonomyId = masterParentTerm.As<TermPart>().TaxonomyId;

                        ModifyTermData(termItem);

                        _notifier.Add(NotifyType.Warning, T("New terms can only be created for the master language ({0}). The term has been automatically moved to the corresponding taxonomy.", masterParentTerm.As<LocalizationPart>().Culture.Culture));
                    }
                    else if (masterTaxonomy != null)
                    {
                        termItem.As<TermPart>().Container = masterTaxonomy;
                        termItem.As<TermPart>().TaxonomyId = masterTaxonomy.Id;

                        ModifyTermData(termItem);

                        _notifier.Add(NotifyType.Warning, T("New terms can only be created for the master language ({0}). The term has been automatically moved to the corresponding taxonomy.", masterTaxonomy.As<LocalizationPart>().Culture.Culture));
                    }
                }
                else
                {
                    var termCulture = termItem.As<LocalizationPart>().Culture;

                    //Se esiste un termine padre, ne cerco la versione localizzata.
                    //Grazie ai controlli presenti nel driver non dovrebbe mai accadere che il padre non abbia una localization part, abbia cultura nulla o non esista una traduzione valida.
                    ContentItem localizedParentTerm = parentTerm;
                    if (parentTerm != null)
                    {
                        if (parentTerm.Has<LocalizationPart>())
                        {
                            var parentTermCulture = parentTerm.As<LocalizationPart>().Culture;

                            if (parentTermCulture != null && termCulture.Id != parentTermCulture.Id)
                            {
                                IContent masterParentTerm = _taxonomyExtensionsServices.GetMasterItem(parentTerm);
                                var localizedParent = _localizationService.GetLocalizedContentItem(masterParentTerm, termCulture.Culture);
                                if (localizedParent != null)
                                    localizedParentTerm = localizedParent.As<ContentItem>();
                            }
                        }
                    }

                    //Cerco la versione localizzata della tassonomia.
                    ContentItem localizedParentTaxonomy = parentTaxonomy;
                    if (parentTaxonomy.Has<LocalizationPart>())
                    {
                        if (parentTaxonomy.As<LocalizationPart>().Culture != null)
                        {
                            var taxonomyCulture = parentTaxonomy.As<LocalizationPart>().Culture;

                            if (termCulture.Id != taxonomyCulture.Id)
                            {
                                IContent masterTaxonomy = _taxonomyExtensionsServices.GetMasterItem(parentTaxonomy);
                                var localizedTaxonomy = _localizationService.GetLocalizedContentItem(masterTaxonomy, termCulture.Culture);
                                if (localizedTaxonomy != null)
                                    localizedParentTaxonomy = localizedTaxonomy.As<ContentItem>();
                            }
                        }
                    }

                    var term = termItem.As<TermPart>();
                    bool translatingTerm = _controllerContextAccessor.Context.RouteData.Values["action"].ToString() == "Translate" && _controllerContextAccessor.Context.Controller.GetType().FullName == "Orchard.Localization.Controllers.AdminController";

                    //Se ho trovato il corrispettivo container localizzato, lo assegno al termine.
                    //Controllo che sia diverso dal termine che sto salvando perchè nella tassonomia master possono essere presenti termini di diverse culture e quindi un termine può essere figlio del proprio master.
                    //Altrimenti, se sto creando una traduzione, imposto come container la tassonomia master.
                    if ((localizedParentTerm == null && localizedParentTaxonomy != parentTaxonomy) || (localizedParentTerm != null && localizedParentTerm != parentTerm && localizedParentTerm.Id != termItem.Id))
                    {
                        term.Container = localizedParentTerm == null ? localizedParentTaxonomy.As<TaxonomyPart>().ContentItem : localizedParentTerm.As<TermPart>().ContentItem;
                        term.Path = localizedParentTerm != null ? localizedParentTerm.As<TermPart>().FullPath + "/" : "/";
                        if (localizedParentTerm == null)
                            term.TaxonomyId = localizedParentTaxonomy.Id;
                        else
                            term.TaxonomyId = localizedParentTerm.As<TermPart>().TaxonomyId;

                        ModifyTermData(termItem);

                        if (localizedParentTaxonomy != parentTaxonomy)
                            _notifier.Add(NotifyType.Information, T("The parent taxonomy has been changed to its localized version associated to the culture {0}.", localizedParentTaxonomy.As<LocalizationPart>().Culture.Culture));

                        if (localizedParentTerm != null && localizedParentTerm != parentTerm)
                            _notifier.Add(NotifyType.Information, T("The parent term has been changed to its localized version associated to the culture {0}.", localizedParentTaxonomy.As<LocalizationPart>().Culture.Culture));
                    }
                    else if (translatingTerm)
                    {
                        IContent parentToAssign = _taxonomyExtensionsServices.GetMasterItem(parentTaxonomy);

                        term.Container = parentToAssign;
                        term.Path = "/";
                        term.TaxonomyId = parentToAssign.Id;

                        ModifyTermData(termItem);

                        _notifier.Add(NotifyType.Warning, T("A localized taxonomy for the culture {0} does not exist. The term has been associated to the master taxonomy ({1}).", termCulture.Culture, parentToAssign.As<LocalizationPart>().Culture.Culture));
                    }
                }
            }
        }

        private void ModifyTermData(ContentItem termItem)
        {
            _taxonomyExtensionsServices.ChangeTermContentType(termItem);
            //_taxonomyExtensionsServices.InheritParentContentCulture(termItem);
            _taxonomyExtensionsServices.RegenerateAutoroute(termItem);
        }
    }
}