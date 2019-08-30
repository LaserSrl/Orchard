using System.Collections.Generic;
using Orchard.Core.Contents.Security;
using Orchard.Security.Permissions;
using Orchard.Users.Models;

namespace Orchard.Users.Security {
    public class ManageUsersPermissionOverrideHandler
        : PermissionOverrideAuthorizationEventHandler<UserPart> {
        protected override IEnumerable<Permission> ReplacedPermissions =>
            BasicCRUDPermissions;

        protected override Permission OverridingPermission =>
            Permissions.ManageUsers;
    }
}