using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using System.Collections.Generic;

namespace Laser.Orchard.Claims.Models {
    public class IdentityClaimsPart : ContentPart<IdentityClaimsPartRecord> {
        public IList<IdentityClaimsRecord> ClaimsSets {
            get {
                return Record.ClaimsSets;
            }
        }
    }

    public class IdentityClaimsPartRecord : ContentPartRecord {
        public IdentityClaimsPartRecord() {
            ClaimsSets = new List<IdentityClaimsRecord>();
        }
        public virtual IList<IdentityClaimsRecord> ClaimsSets { get; set; }
    }
}