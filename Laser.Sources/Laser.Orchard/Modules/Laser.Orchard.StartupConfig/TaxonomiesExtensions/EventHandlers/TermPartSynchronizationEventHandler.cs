using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using Laser.Orchard.StartupConfig.TaxonomiesExtensions.Events;
using Laser.Orchard.StartupConfig.TaxonomiesExtensions.Models;

namespace Laser.Orchard.StartupConfig.TaxonomiesExtensions.EventHandlers
{
    /// <summary>
    /// Further synchronization for Taxonomy terms.
    /// </summary>
    [OrchardFeature("Laser.Orchard.StartupConfig.TaxonomiesExtensions")]
    public class TermPartSynchronizationEventHandler : ILocalizationExtensionEventHandler
    {
        private readonly ITaxonomyService _taxonomyService;


        public TermPartSynchronizationEventHandler(ITaxonomyService taxonomyService)
        {
            _taxonomyService = taxonomyService;
        }


        public void ContainerSynchronized(IContainerSynchronizedContext containerSynchronizedContext)
        {
            var termPart = containerSynchronizedContext.ContentItem.As<TermPart>();
            if (termPart == null) return;

            var localizedMasterContentItemContainer = containerSynchronizedContext.LocalizedMasterContentItemContainer;

            var localizedTaxonomyId = localizedMasterContentItemContainer.As<TaxonomyPart>() != null ? localizedMasterContentItemContainer.As<TaxonomyPart>().Id : localizedMasterContentItemContainer.As<TermPart>().TaxonomyId;

            termPart.TaxonomyId = localizedTaxonomyId;
            termPart.Container = localizedMasterContentItemContainer;
            _taxonomyService.ProcessPath(termPart);
        }

        public void SynchronizingDataToLocalizedVersions(ISynchronizingDataToLocalizedVersionsContext synchronizingDataToLocalizedVersionsContext) { }
    }
}