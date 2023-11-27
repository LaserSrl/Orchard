﻿using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI.WebControls;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Environment.Extensions;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using Orchard.Taxonomies.Drivers;
using Orchard.Taxonomies.Helpers;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using Orchard.Taxonomies.Settings;
using Orchard.Taxonomies.ViewModels;
using Orchard.UI.Admin;

namespace Orchard.Taxonomies.Controllers {
    [OrchardFeature("Orchard.Taxonomies.LocalizationExtensions")]
    public class LocalizedTaxonomyController : Controller {

        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ILocalizationService _localizationService;
        private readonly ITaxonomyService _taxonomyService;
        private readonly ITaxonomyExtensionsService _taxonomyExtensionsService;
        private readonly RequestContext _requestContext;

        public LocalizedTaxonomyController(
            IContentDefinitionManager contentDefinitionManager,
            ILocalizationService localizationService,
            ITaxonomyService taxonomyService,
            ITaxonomyExtensionsService taxonomyExtensionsService,
            RequestContext requestContext) {

            _taxonomyService = taxonomyService;
            _taxonomyExtensionsService = taxonomyExtensionsService;
            _contentDefinitionManager = contentDefinitionManager;
            _localizationService = localizationService;
            _requestContext = requestContext;
        }

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult GetTaxonomy(string contentTypeName, string taxonomyFieldName, int contentId, string culture, string selectedValues, bool isAdmin = false) {

            if (isAdmin) {
                AdminFilter.Apply(_requestContext);
            }

            var viewModel = new TaxonomyFieldViewModel();
            bool autocomplete = false;
            var contentDefinition = _contentDefinitionManager.GetTypeDefinition(contentTypeName);
            if (contentDefinition != null) {
                var taxonomyField = contentDefinition.Parts.SelectMany(p => p.PartDefinition.Fields).Where(x => x.FieldDefinition.Name == "TaxonomyField" && x.Name == taxonomyFieldName).FirstOrDefault();
                var contentTypePartDefinition = contentDefinition.Parts.Where(x => x.PartDefinition.Fields.Any(a => a.FieldDefinition.Name == "TaxonomyField" && a.Name == taxonomyFieldName)).FirstOrDefault();
                var fieldPrefix = contentTypePartDefinition.PartDefinition.Name + "." + taxonomyField.Name;
                ViewData.TemplateInfo.HtmlFieldPrefix = fieldPrefix;
                if (taxonomyField != null) {
                    var taxonomySettings = taxonomyField.Settings.GetModel<TaxonomyFieldSettings>();
                    // Getting the translated taxonomy and its terms

                    var masterTaxonomy = _taxonomyExtensionsService.GetMasterItem(_taxonomyService.GetTaxonomyByName(taxonomySettings.Taxonomy));
                    IContent taxonomy;
                    var trytranslate = _localizationService.GetLocalizedContentItem(masterTaxonomy, culture);
                    if (trytranslate == null) // case taxonomy not localized
                        taxonomy = masterTaxonomy;
                    else
                        taxonomy = _localizationService.GetLocalizedContentItem(masterTaxonomy, culture).ContentItem;
                    var terms = taxonomy != null // && !taxonomySettings.Autocomplete
                        ? _taxonomyService.GetTerms(taxonomy.Id).Where(t => !string.IsNullOrWhiteSpace(t.Name)).Select(t => t.CreateTermEntry()).Where(te => !te.HasDraft).ToList()
                        : new List<TermEntry>(0);
                    List<TermPart> appliedTerms = new List<TermPart>();
                    int firstTermIdForCulture = 0;
                    if (contentId > 0) {
                        //if (string.IsNullOrWhiteSpace(selectedValues)) {
                        //    appliedTerms = _taxonomyService.GetTermsForContentItem(contentId, taxonomyFieldName, VersionOptions.Published).Distinct(new TermPartComparer()).ToList();
                        //} else {
                        var selectedIds = selectedValues.Split(',');
                        foreach (var id in selectedIds) {
                            if (!string.IsNullOrWhiteSpace(id)) {
                                var intId = 0;
                                int.TryParse(id, out intId);
                                var t = _localizationService.GetLocalizedContentItem(_taxonomyService.GetTerm(intId), culture);
                                if (t != null) {
                                    appliedTerms.Add(t.As<TermPart>());
                                }
                            }
                        }
                        //}
                        // It takes the first term localized with the culture in order to set correctly the TaxonomyFieldViewModel.SingleTermId
                        var firstTermForCulture = appliedTerms.FirstOrDefault(x => x.As<LocalizationPart>() != null && x.As<LocalizationPart>().Culture != null && x.As<LocalizationPart>().Culture.Culture == culture);
                        if (firstTermForCulture != null) {
                            firstTermIdForCulture = firstTermForCulture.Id;
                        } else {
                            // If there is no valid localization, firstTermForCulture is null.
                            // To avoid that, use the first checked term (if any is checked).
                            firstTermForCulture = appliedTerms.FirstOrDefault(t => terms.Any(x => x.Id == t.Id));
                            if (firstTermForCulture != null) {
                                firstTermIdForCulture = firstTermForCulture.Id;
                            }
                        }
                        terms.ForEach(t => t.IsChecked = appliedTerms.Any(x => x.Id == t.Id));
                    }
                    viewModel = new TaxonomyFieldViewModel {
                        DisplayName = taxonomyField.DisplayName,
                        Name = taxonomyField.Name,
                        Terms = terms,
                        SelectedTerms = appliedTerms,
                        Settings = taxonomySettings,
                        SingleTermId = firstTermIdForCulture,
                        TaxonomyId = taxonomy != null ? taxonomy.Id : 0,
                        HasTerms = taxonomy != null && _taxonomyService.GetTermsCount(taxonomy.Id) > 0
                    };
                    if (taxonomySettings.Autocomplete) {
                        autocomplete = true;
                    }
                }
            }
            var templateName = autocomplete ? "../EditorTemplates/Fields/TaxonomyField.Autocomplete" : "../EditorTemplates/Fields/TaxonomyField";
            return View(templateName, viewModel);
        }

    }
}
