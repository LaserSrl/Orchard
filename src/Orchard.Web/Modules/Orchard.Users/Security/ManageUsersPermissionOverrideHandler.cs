using System.Collections.Generic;
using Orchard.Core.Contents.Security;
using Orchard.Security.Permissions;
using Orchard.Users.Models;

namespace Orchard.Users.Security {
    public class ManageUsersPermissionOverrideHandler
        : PermissionOverrideAuthorizationEventHandler<UserPart> {
        protected override IEnumerable<Permission> ReplacedPermissions =>
            new List<Permission>() {
                Core.Contents.Permissions.CreateContent,
                Core.Contents.Permissions.EditContent,
                Core.Contents.Permissions.PublishContent,
                Core.Contents.Permissions.DeleteContent,
            };

        protected override Permission OverridingPermission =>
            Permissions.ManageUsers;
    }
}