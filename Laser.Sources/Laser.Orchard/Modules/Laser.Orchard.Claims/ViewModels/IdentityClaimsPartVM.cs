using Laser.Orchard.Claims.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Laser.Orchard.Claims.ViewModels {
    public class IdentityClaimsPartVM {
        public IdentityClaimsPartVM(IdentityClaimsPart part) {
            IdentityClaims = part;
            var sb = new StringBuilder();
            foreach (var row in part.ClaimsSets) {
                sb.AppendLine(row.IdentityClaims);
            }
            ClaimsArea = sb.ToString();
        }
        public IdentityClaimsPart IdentityClaims { get; set; }
        public string ClaimsArea { get; set; }
    }
}