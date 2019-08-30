using System.Collections.Generic;
using Orchard.Core.Contents.Security;
using Orchard.Security;
using Orchard.Security.Permissions;

namespace Orchard.Core.Settings.Security {
    public class ManageSettingsPermissionOverrideHandler
        : PermissionOverrideAuthorizationEventHandler {
        protected override IEnumerable<Permission> ReplacedPermissions =>
            BasicCRUDPermissions;

        protected override Permission OverridingPermission =>
            Permissions.ManageSettings;

        protected override bool ShouldOverride(CheckAccessContext context) {
            return base.ShouldOverride(context)
                && context.Content != null
                && context.Content.ContentItem.ContentType == "Site";
        }
    }
}