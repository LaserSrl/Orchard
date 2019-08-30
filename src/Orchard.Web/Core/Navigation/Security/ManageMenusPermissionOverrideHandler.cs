﻿using System.Collections.Generic;
using Orchard.Core.Contents.Security;
using Orchard.Security;
using Orchard.Security.Permissions;

namespace Orchard.Core.Navigation.Security {
    public class ManageMenusPermissionOverrideHandler
        : PermissionOverrideAuthorizationEventHandler {
        protected override IEnumerable<Permission> ReplacedPermissions =>
            BasicCRUDPermissions;

        protected override Permission OverridingPermission =>
            Permissions.ManageMenus;

        protected override bool ShouldOverride(CheckAccessContext context) {
            if (base.ShouldOverride(context) && context.Content != null) {
                string stereotype;
                var item = context.Content.ContentItem;
                return item.ContentType == "Menu"
                    || (item.TypeDefinition.Settings.TryGetValue("Stereotype", out stereotype)
                        && stereotype == "MenuItem");
            }
            return false;
        }
    }
}