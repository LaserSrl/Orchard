using System.Collections.Generic;
using Orchard.Core.Contents.Security;
using Orchard.Projections.Models;
using Orchard.Security.Permissions;

namespace Orchard.Projections.Security {
    public class ManageQueriesPermissionOverrideHandler
        : PermissionOverrideAuthorizationEventHandler<QueryPart> {
        protected override IEnumerable<Permission> ReplacedPermissions =>
            new List<Permission>() {
                Core.Contents.Permissions.CreateContent,
                Core.Contents.Permissions.EditContent,
                Core.Contents.Permissions.PublishContent,
                Core.Contents.Permissions.DeleteContent,
            };

        protected override Permission OverridingPermission =>
            Permissions.ManageQueries;
    }
}