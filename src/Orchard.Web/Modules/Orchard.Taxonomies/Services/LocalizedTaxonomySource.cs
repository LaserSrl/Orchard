using System;
using System.Linq;
using System.Web.Management;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.Environment.Extensions;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using Orchard.Taxonomies.Models;


namespace Orchard.Taxonomies.Services {
    [OrchardFeature("Orchard.Taxonomies.LocalizationExtensions")]
    public class LocalizedTaxonomySource : ITaxonomySource {
        private readonly ILocalizationService _localizationService;
        private readonly IContentManager _contentManager;
        public LocalizedTaxonomySource(
            ILocalizationService localizationService,
            IContentManager contentManager) {
            _localizationService = localizationService;
            _contentManager = contentManager;
        }

        public TaxonomyPart GetTaxonomy(string name, ContentItem currentcontent) {
            if (String.IsNullOrWhiteSpace(name)) {
                throw new ArgumentNullException("name");
            }
            string culture = _localizationService.GetContentCulture(currentcontent);
            var taxonomyPart = _contentManager.Query<TaxonomyPart, TaxonomyPartRecord>()
                .Join<TitlePartRecord>()
                .Where(r => r.Title == name)
                .List()
                .FirstOrDefault();
            // Null check on taxonomyPart
            // It can be null in the case of a TaxonomyField with not taxonomy selected (misconfiguration).
            if (taxonomyPart == null) {
                return null;
            }

            // If current content isn't localized, check for its MasterContentItem.
            var localized = currentcontent.As<LocalizationPart>();
            var c = localized?.Culture?.Culture;
            if (string.IsNullOrWhiteSpace(c)) {
                var master = localized.MasterContentItem;
                if (master != null) {
                    c = master.As<LocalizationPart>()?.Culture?.Culture;
                }
            }

            // If there is no MasterContentItem (or if it's not localized), return the original TaxonomyPart.
            if (string.IsNullOrWhiteSpace(c)) {
                return taxonomyPart;
            }

            // If previous checks have been passed, get the localized version of the TaxonomyPart.
            return _localizationService.GetLocalizedContentItem(taxonomyPart.ContentItem, c).As<TaxonomyPart>() ?? taxonomyPart;
        }
    }
}