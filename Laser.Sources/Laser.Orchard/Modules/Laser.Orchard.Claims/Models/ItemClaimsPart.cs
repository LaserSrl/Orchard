using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace Laser.Orchard.Claims.Models {
    public class ItemClaimsPart : ContentPart<ItemClaimsPartRecord> {
        public string Claims {
            get { return Retrieve(r => r.Claims); }
            set { Store(r => r.Claims, value); }
        }
    }

    public class ItemClaimsPartRecord : ContentPartVersionRecord {
        public virtual string Claims { get; set; }
    }
}