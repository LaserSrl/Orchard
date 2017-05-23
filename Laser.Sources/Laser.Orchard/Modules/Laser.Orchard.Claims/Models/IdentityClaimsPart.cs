using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.ContentManagement.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Claims.Models {
    public class IdentityClaimsPart : ContentPart<IdentityClaimsPartRecord> {
        internal readonly LazyField<IdentityClaimsRecord> IdentityClaimsField;
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