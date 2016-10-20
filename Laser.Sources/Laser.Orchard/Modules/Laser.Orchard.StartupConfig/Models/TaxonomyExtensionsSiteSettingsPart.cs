using Orchard.ContentManagement;

namespace Laser.Orchard.StartupConfig.Models
{
    public class TaxonomyExtensionsSiteSettingsPart : ContentPart
    {
        public bool LocalizeTerms
        {
            get { return this.Retrieve(x => x.LocalizeTerms, true); }
            set { this.Store(x => x.LocalizeTerms, value); }
        }
    }
}