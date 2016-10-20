using Orchard.ContentManagement;

namespace Laser.Orchard.StartupConfig.TaxonomiesExtensions.Models
{
    public class ContainerSynchronizedContext : IContainerSynchronizedContext
    {
        public IContent ContentItem { get; set; }

        public IContent LocalizedMasterContentItemContainer { get; set; }
    }
}