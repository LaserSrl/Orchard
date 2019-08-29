using System.Collections.Generic;
using Orchard.Blogs.Models;
using Orchard.Core.Contents.Security;
using Orchard.Security.Permissions;

namespace Orchard.Blogs.Security {
    public class ManageBlogsPermissionOverrideHandler
        : PermissionOverrideAuthorizationEventHandler<BlogPart> {
        protected override IEnumerable<Permission> ReplacedPermissions =>
            new List<Permission>() {
                Core.Contents.Permissions.CreateContent,
                Core.Contents.Permissions.EditContent,
                Core.Contents.Permissions.PublishContent,
                Core.Contents.Permissions.DeleteContent,
            };

        protected override Permission OverridingPermission =>
            Permissions.ManageBlogs;
    }
}