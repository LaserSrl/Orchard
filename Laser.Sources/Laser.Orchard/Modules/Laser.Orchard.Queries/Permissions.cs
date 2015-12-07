using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Security.Permissions;
using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;

namespace Laser.Orchard.Queries {
    public class Permissions: IPermissionProvider{
        public static readonly Permission CustomQuery = new Permission { Description = "Manage CustomQuery", Name = "CustomQuery" };
      
        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                CustomQuery
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                 Permissions = new[] {CustomQuery}
                },
                new PermissionStereotype {
                    Name = "Editor",
                },
                new PermissionStereotype {
                    Name = "Moderator",
                  },
                new PermissionStereotype {
                    Name = "Author",
                },
                new PermissionStereotype {
                    Name = "Contributor",
                },
            };
        }
    }
}