using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Claims.Models {
    public class IdentityClaimsRecord {
        public virtual int Id { get; set; }
        public virtual string IdentityClaims { get; set; }
        public virtual int IdentityClaimsPartRecord_id { get; set; }
    }
}