﻿using System.Collections.Generic;
using Orchard.Comments.Models;
using Orchard.Core.Contents.Security;
using Orchard.Security.Permissions;

namespace Orchard.Comments.Security {
    public class ManageCommentsPermissionOverrideHandler
        : PermissionOverrideAuthorizationEventHandler<CommentPart> {
        protected override IEnumerable<Permission> ReplacedPermissions =>
            BasicCRUDPermissions;

        protected override Permission OverridingPermission =>
            Permissions.ManageComments;
    }
}