using Orchard.ContentManagement;

namespace Laser.Orchard.Claims.Models {
    public class ClaimsSiteSettings : ContentPart {
        public bool ApplyToFrontEnd {
            get { return this.Retrieve(x => x.ApplyToFrontEnd); }
            set { this.Store(x => x.ApplyToFrontEnd, value); }
        }
    }
}