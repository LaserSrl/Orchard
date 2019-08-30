﻿using System.Collections.Generic;
using Orchard.Blogs.Models;
using Orchard.Core.Contents.Security;
using Orchard.Security.Permissions;

namespace Orchard.Blogs.Security {
    public class ManageBlogsPermissionOverrideHandler
        : PermissionOverrideAuthorizationEventHandler<BlogPart> {
        protected override IEnumerable<Permission> ReplacedPermissions =>
            BasicCRUDPermissions;

        protected override Permission OverridingPermission =>
            Permissions.ManageBlogs;
    }
}