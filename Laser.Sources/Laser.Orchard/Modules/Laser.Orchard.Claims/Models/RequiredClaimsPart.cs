using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace Laser.Orchard.Claims.Models {
    public class RequiredClaimsPart : ContentPart<RequiredClaimsPartRecord> {
        public string Claims {
            get { return Retrieve(r => r.Claims); }
            set { Store(r => r.Claims, value); }
        }
    }

    public class RequiredClaimsPartRecord : ContentPartVersionRecord {
        public virtual string Claims { get; set; }
    }
}