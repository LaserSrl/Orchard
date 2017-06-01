using Orchard.Security.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Environment.Extensions.Models;

namespace Laser.Orchard.Claims.Security {
    public class ClaimsPermissions : IPermissionProvider {
        public static readonly Permission EditClaims = new Permission { Description = "Edit Claims", Name = "EditClaims" };
        //public static readonly Permission ApplyClaimsOnVisibility = new Permission { Description = "Apply Claims on item visibility", Name = "ApplyClaimsOnVisibility" };
        //public static readonly Permission ApplyClaimsOnManagement = new Permission { Description = "Apply Claims on item management", Name = "ApplyClaimsOnManagement", ImpliedBy = new[] { ApplyClaimsOnVisibility } };
        public Feature Feature { get; set; }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] { EditClaims }
                }
            };
        }

        public IEnumerable<Permission> GetPermissions() {
            //return new [] { EditClaims, ApplyClaimsOnVisibility, ApplyClaimsOnManagement };
            return new[] { EditClaims };
        }
    }
}