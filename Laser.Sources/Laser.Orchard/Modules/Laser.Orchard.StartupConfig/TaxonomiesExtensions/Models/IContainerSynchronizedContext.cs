using Orchard.ContentManagement;

namespace Laser.Orchard.StartupConfig.TaxonomiesExtensions.Models
{
    /// <summary>
    /// Context for container synchronized events.
    /// </summary>
    public interface IContainerSynchronizedContext
    {
        /// <summary>
        /// The currently created content item.
        /// </summary>
        IContent ContentItem { get; }

        /// <summary>
        /// The container of the content item's master content item's localized version.
        /// </summary>
        IContent LocalizedMasterContentItemContainer { get; }
    }
}