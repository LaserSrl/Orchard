using Orchard.ContentManagement;
using System.Collections.Generic;

namespace Laser.Orchard.StartupConfig.TaxonomiesExtensions.Models
{
    /// <summary>
    /// Context for synchronizing data to localized versions events.
    /// </summary>
    public interface ISynchronizingDataToLocalizedVersionsContext
    {
        /// <summary>
        /// The currently created content item.
        /// </summary>
        IContent ContentItem { get; set; }

        /// <summary>
        /// List of the localized versions.
        /// </summary>
        IEnumerable<IContent> LocalizedVersions { get; set; }
    }
}
