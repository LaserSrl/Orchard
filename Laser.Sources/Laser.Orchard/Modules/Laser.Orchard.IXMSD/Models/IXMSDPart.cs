using Orchard.ContentManagement;

namespace Laser.Orchard.IXMSD.Models {

    public class IXMSDPart : ContentPart {

        public string ExternalMediaUrl {
            get { return this.Retrieve(x => x.ExternalMediaUrl); }
            set { this.Store(x => x.ExternalMediaUrl, value); }
        }
    }
}