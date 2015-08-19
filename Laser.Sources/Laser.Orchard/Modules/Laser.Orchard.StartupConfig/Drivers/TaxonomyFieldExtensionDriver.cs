using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Title.Models;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using Orchard.Taxonomies.Fields;
using Orchard.Taxonomies.Helpers;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using Orchard.Taxonomies.Settings;
using Orchard.Taxonomies.ViewModels;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using OrchardCore = Orchard;

namespace Laser.Orchard.StartupConfig.Drivers
{

    [OrchardFeature("Laser.Orchard.StartupConfig.TaxonomiesExtensions")]
    [OrchardSuppressDependency("Orchard.Taxonomies.Drivers.TaxonomyFieldDriver")]
    public class TaxonomyFieldExtensionDriver : ContentFieldDriver<TaxonomyField>
    {
        private readonly ITaxonomyService _taxonomyService;
        private readonly ILocalizationService _localizationService;
        private readonly ICultureManager _cultureManager;
        public IOrchardServices Services { get; set; }


        public TaxonomyFieldExtensionDriver(
            ICultureManager cultureManager,
            IOrchardServices services,
            ITaxonomyService taxonomyService,
            IRepository<TermContentItem> repository,
            ILocalizationService localizationService)
        {
            _cultureManager = cultureManager;
            _taxonomyService = taxonomyService;
            Services = services;
            T = NullLocalizer.Instance;
            _localizationService = localizationService;
        }

        public Localizer T { get; set; }

        private static string GetPrefix(ContentField field, ContentPart part)
        {
            return part.PartDefinition.Name + "." + field.Name;
        }

        private static string GetDifferentiator(TaxonomyField field, ContentPart part)
        {
            return field.Name;
        }

        protected override DriverResult Display(ContentPart part, TaxonomyField field, string displayType, dynamic shapeHelper)
        {

            return ContentShape("Fields_TaxonomyField_Localized", GetDifferentiator(field, part),
                () =>
                {
                    var settings = field.PartFieldDefinition.Settings.GetModel<TaxonomyFieldSettings>();
                    var terms = _taxonomyService.GetTermsForContentItem(part.ContentItem.Id, field.Name).ToList();
                    var taxonomy = _taxonomyService.GetTaxonomyByName(settings.Taxonomy);

                    return shapeHelper.Fields_TaxonomyField(
                        ContentField: field,
                        Terms: terms,
                        Settings: settings,
                        Taxonomy: taxonomy);
                });
        }

        protected override DriverResult Editor(ContentPart part, TaxonomyField field, dynamic shapeHelper)
        {
            return ContentShape("Fields_TaxonomyField_Localized_Edit", GetDifferentiator(field, part), () =>
            {
                var settings = field.PartFieldDefinition.Settings.GetModel<TaxonomyFieldSettings>();
                var taxonomyName = settings.Taxonomy;
                var taxonomyLocalizedName = "";
                var taxonomy = _taxonomyService.GetTaxonomyByName(settings.Taxonomy);

                /*Localized context*/
                if (part.ContentItem.As<LocalizationPart>() != null && part.ContentItem.As<LocalizationPart>().Culture != null && part.ContentItem.As<LocalizationPart>().Culture.Culture != Services.WorkContext.CurrentSite.SiteCulture)
                {
                    var translatedTaxonomy = _localizationService.GetLocalizedContentItem(taxonomy, part.ContentItem.As<LocalizationPart>().Culture.Culture);
                    if (translatedTaxonomy != null)
                    {
                        taxonomyLocalizedName = translatedTaxonomy.As<TitlePart>().Title;
                        taxonomy = _taxonomyService.GetTaxonomyByName(taxonomyLocalizedName);
                    }
                }
                var appliedTerms = new Dictionary<int, TermPart>();
                try
                {
                    appliedTerms = _taxonomyService.GetTermsForContentItem(part.ContentItem.Id, field.Name).Distinct(new TermPartComparer()).ToDictionary(t => t.Id, t => t);
                }
                catch { } // Se per qualche ragione il termine selezionato non è disponibile, deve comunque mostrare il campo anche se senza selezione

                var terms = taxonomy != null
                    ? _taxonomyService.GetTerms(taxonomy.Id).Where(t => !string.IsNullOrWhiteSpace(t.Name)).Select(t => t.CreateTermEntry()).ToList()
                    : new List<TermEntry>(0);

                terms.ForEach(t => t.IsChecked = appliedTerms.ContainsKey(t.Id));

                var viewModel = new TaxonomyFieldViewModel
                {
                    DisplayName = field.DisplayName,
                    Name = field.Name,
                    Terms = terms,
                    Settings = settings,
                    SingleTermId = terms.Where(t => t.IsChecked).Select(t => t.Id).FirstOrDefault(),
                    TaxonomyId = taxonomy != null ? taxonomy.Id : 0
                };

                var templateName = settings.Autocomplete ? "Fields/TaxonomyField.Autocomplete" : "Fields/TaxonomyField";
                return shapeHelper.EditorTemplate(TemplateName: templateName, Model: viewModel, Prefix: GetPrefix(field, part));
            });
        }

        protected override DriverResult Editor(ContentPart part, TaxonomyField field, IUpdateModel updater, dynamic shapeHelper)
        {
            var viewModel = new TaxonomyFieldViewModel { Terms = new List<TermEntry>() };

            if (updater.TryUpdateModel(viewModel, GetPrefix(field, part), null, null))
            {
                var checkedTerms = viewModel.Terms
                    .Where(t => (t.IsChecked || t.Id == viewModel.SingleTermId))
                    .Select(t => GetOrCreateTerm(t, viewModel.TaxonomyId, field))
                    .Where(t => t != null).ToList();

                /*Localized context*/
                /*Verifico se sto arrivando da una traduzione*/
                var settingsTax = field.PartFieldDefinition.Settings.GetModel<TaxonomyFieldSettings>();
                var taxonomyName = settingsTax.Taxonomy;
                var taxonomyLocalizedName = "";
                var taxonomy = _taxonomyService.GetTaxonomyByName(settingsTax.Taxonomy);
                if (part.ContentItem.As<LocalizationPart>() != null)
                {
                    var translateModel = new OrchardCore.Localization.ViewModels.EditLocalizationViewModel();
                    if (updater.TryUpdateModel(translateModel, null, null, null))
                    {
                        if (checkedTerms.Any())
                        {
                            var translatedTaxonomy = _localizationService.GetLocalizedContentItem(taxonomy, translateModel.SelectedCulture);
                            if (translatedTaxonomy != null && checkedTerms[0].Has<LocalizationPart>())
                            {
                                var termsTaxId = checkedTerms[0].TaxonomyId;
                                taxonomyLocalizedName = translatedTaxonomy.As<TitlePart>().Title;
                                taxonomy = _taxonomyService.GetTaxonomyByName(taxonomyLocalizedName);
                                if (termsTaxId != taxonomy.Id)
                                {
                                    List<TermPart> translatedCheckedTerms = new List<TermPart>();
                                    bool missingTerms = false;

                                    foreach (var term in checkedTerms)
                                    {
                                        var translatedTerm = _localizationService.GetLocalizedContentItem(term, translateModel.SelectedCulture);
                                        if (translatedTerm != null)
                                            translatedCheckedTerms.Add(translatedTerm.As<TermPart>());
                                        else
                                            missingTerms = true;
                                    }

                                    checkedTerms = translatedCheckedTerms;

                                    if (missingTerms)
                                        Services.Notifier.Add(NotifyType.Information, T("The checked terms have been moved to the translated taxonomy, but some of them didn't have an adequate localization and were removed."));
                                    else
                                        Services.Notifier.Add(NotifyType.Information, T("The checked terms have been moved to the translated taxonomy."));
                                }
                            }
                        }
                    }
                }

                var settings = field.PartFieldDefinition.Settings.GetModel<TaxonomyFieldSettings>();
                if (settings.Required && !checkedTerms.Any())
                {
                    updater.AddModelError(GetPrefix(field, part), T("The field {0} is mandatory.", T(field.DisplayName)));
                }
                else
                    _taxonomyService.UpdateTerms(part.ContentItem, checkedTerms, field.Name);
            }

            return Editor(part, field, shapeHelper);
        }

        protected override void Exporting(ContentPart part, TaxonomyField field, ExportContentContext context)
        {
            var appliedTerms = _taxonomyService.GetTermsForContentItem(part.ContentItem.Id, field.Name);

            // stores all content items associated to this field
            var termIdentities = appliedTerms.Select(x => Services.ContentManager.GetItemMetadata(x).Identity.ToString())
                .ToArray();

            context.Element(XmlConvert.EncodeLocalName(field.FieldDefinition.Name + "." + field.Name)).SetAttributeValue("Terms", String.Join(",", termIdentities));
        }

        protected override void Importing(ContentPart part, TaxonomyField field, ImportContentContext context)
        {
            var termIdentities = context.Attribute(XmlConvert.EncodeLocalName(field.FieldDefinition.Name + "." + field.Name), "Terms");
            if (termIdentities == null)
            {
                return;
            }

            var terms = termIdentities
                            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(context.GetItemFromSession)
                            .Where(contentItem => contentItem != null)
                            .ToList();

            _taxonomyService.UpdateTerms(part.ContentItem, terms.Select(x => x.As<TermPart>()), field.Name);
        }

        private TermPart GetOrCreateTerm(TermEntry entry, int taxonomyId, TaxonomyField field)
        {
            var term = entry.Id > 0 ? _taxonomyService.GetTerm(entry.Id) : default(TermPart);

            if (term == null)
            {
                var settings = field.PartFieldDefinition.Settings.GetModel<TaxonomyFieldSettings>();

                if (!settings.AllowCustomTerms || !Services.Authorizer.Authorize(OrchardCore.Taxonomies.Permissions.CreateTerm))
                {
                    Services.Notifier.Error(T("You're not allowed to create new terms for this taxonomy"));
                    return null;
                }

                var taxonomy = _taxonomyService.GetTaxonomy(taxonomyId);
                term = _taxonomyService.NewTerm(taxonomy);
                term.Container = taxonomy.ContentItem;
                term.Name = entry.Name.Trim();
                term.Selectable = true;

                _taxonomyService.ProcessPath(term);
                Services.ContentManager.Create(term, VersionOptions.Published);
                Services.Notifier.Information(T("The {0} term has been created.", term.Name));
            }

            return term;
        }
    }

    internal class TermPartComparer : IEqualityComparer<TermPart>
    {
        public bool Equals(TermPart x, TermPart y)
        {
            return x.Id.Equals(y.Id);
        }

        public int GetHashCode(TermPart obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
