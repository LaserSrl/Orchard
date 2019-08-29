using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Core.Contents.Security;
using Orchard.MediaLibrary.Models;
using Orchard.Security.Permissions;

namespace Orchard.MediaLibrary.Security {
    public class DeleteMediaContentPermissionOverrideHandlers
        : PermissionOverrideAuthorizationEventHandler<MediaPart> {
        protected override IEnumerable<Permission> ReplacedPermissions =>
            new List<Permission>() {
                Core.Contents.Permissions.DeleteContent,
            };

        protected override Permission OverridingPermission =>
            Permissions.DeleteMediaContent;
    }
    public class EditMediaContentPermissionOverrideHandlers
       : PermissionOverrideAuthorizationEventHandler<MediaPart> {
        protected override IEnumerable<Permission> ReplacedPermissions =>
            new List<Permission>() {
                Core.Contents.Permissions.CreateContent,
                Core.Contents.Permissions.EditContent,
                Core.Contents.Permissions.PublishContent
            };

        protected override Permission OverridingPermission =>
            Permissions.EditMediaContent;
    }
    public class ManageMediaContentPermissionOverrideHandlers
       : PermissionOverrideAuthorizationEventHandler<MediaPart> {
        protected override IEnumerable<Permission> ReplacedPermissions =>
            new List<Permission>() {
                Core.Contents.Permissions.CreateContent,
                Core.Contents.Permissions.EditContent,
                Core.Contents.Permissions.PublishContent,
                Core.Contents.Permissions.DeleteContent,
            };

        protected override Permission OverridingPermission =>
            Permissions.ManageMediaContent;
    }
}