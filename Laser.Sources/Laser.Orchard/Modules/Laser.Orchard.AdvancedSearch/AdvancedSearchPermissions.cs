using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace Laser.Orchard.AdvancedSearch {
    public class AdvancedSearchPermissions : IPermissionProvider {

        //With this permission, a user is able to visualize exclusively their own contents. They are unable to see anything they do not own.
        public static readonly Permission CanOnlySeeOwnContents = new Permission {
            Description = "A user with this permission can see exclusively the content they own.",
            Name = "CanOnlySeeOwnContents"
        };
        //With this permission, a user initially can only visualize their own contents, but may choose to be able to see contents belonging
        //to other users.
        public static readonly Permission MayChooseToSeeOthersContent = new Permission {
            Description = "A user with this permission may choose to see other users' contents, but does not by default.",
            Name = "MayChooseToSeeOthersContent"
        };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                CanOnlySeeOwnContents,
                MayChooseToSeeOthersContent
            };
        }

        //must implement GetDefaultStereotypes() even though we have no stereotypes
        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return Enumerable.Empty<PermissionStereotype>();
        }
    }
}