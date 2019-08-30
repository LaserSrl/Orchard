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
        private static Permission[] replaced = {
            Core.Contents.Permissions.DeleteContent
        };
        protected override IEnumerable<Permission> ReplacedPermissions =>
            replaced;

        protected override Permission OverridingPermission =>
            Permissions.DeleteMediaContent;
    }
    public class EditMediaContentPermissionOverrideHandlers
       : PermissionOverrideAuthorizationEventHandler<MediaPart> {
        private static Permission[] replaced = {
            Core.Contents.Permissions.CreateContent,
            Core.Contents.Permissions.EditContent,
            Core.Contents.Permissions.PublishContent
        };
        protected override IEnumerable<Permission> ReplacedPermissions =>
            replaced;

        protected override Permission OverridingPermission =>
            Permissions.EditMediaContent;
    }
    public class ManageMediaContentPermissionOverrideHandlers
       : PermissionOverrideAuthorizationEventHandler<MediaPart> {
        private static Permission[] replaced = {
            Core.Contents.Permissions.CreateContent,
            Core.Contents.Permissions.EditContent,
            Core.Contents.Permissions.PublishContent,
            Core.Contents.Permissions.DeleteContent,
        };
        protected override IEnumerable<Permission> ReplacedPermissions =>
            replaced;

        protected override Permission OverridingPermission =>
            Permissions.ManageMediaContent;
    }
}