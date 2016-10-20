using Orchard.ContentManagement;
using System.Collections.Generic;

namespace Laser.Orchard.StartupConfig.TaxonomiesExtensions.Models
{
    public class SynchronizingDataToLocalizedVersionsContext : ISynchronizingDataToLocalizedVersionsContext
    {
        public IContent ContentItem { get; set; }

        public IEnumerable<IContent> LocalizedVersions { get; set; }
    }
}