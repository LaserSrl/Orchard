using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;

namespace Orchard.Security.Permissions {
    public class Permission {
        public Permission() {
            ReplaceFor = new PermissionReplaceContext {
                ReplacedPermissions = new List<Permission>(),
                Condition = (permission, context) => false,
                OverrideSecurable = false
            };
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }

        public IEnumerable<Permission> ImpliedBy { get; set; }
        public bool RequiresOwnership { get; set; }

        public PermissionReplaceContext ReplaceFor { get; set; }

        public static Permission Named(string name) {
            return new Permission { Name = name };
        }
    }

    public class PermissionReplaceContext {
        public IEnumerable<Permission> ReplacedPermissions { get; set; }
        public bool OverrideSecurable { get; set; }
        public Func<Permission, IContent, bool> Condition { get; set; }
    }
}