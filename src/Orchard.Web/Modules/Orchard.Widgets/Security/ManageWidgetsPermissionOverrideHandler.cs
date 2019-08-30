﻿using System.Collections.Generic;
using Orchard.Core.Contents.Security;
using Orchard.Security.Permissions;
using Orchard.Widgets.Models;

namespace Orchard.Widgets.Security {
    public class ManageWidgetsPermissionOverrideHandler
        : PermissionOverrideAuthorizationEventHandler<WidgetPart> {
        // This class is identical to ManageLayersPermissionOverrideHandler
        // because both WidgetPart and LayerPart are protected by the same Permission.
        protected override IEnumerable<Permission> ReplacedPermissions =>
            BasicCRUDPermissions;

        protected override Permission OverridingPermission =>
            Permissions.ManageWidgets;
    }

    public class ManageLayersPermissionOverrideHandler
        : PermissionOverrideAuthorizationEventHandler<LayerPart> {
        // This class is identical to ManageWidgetsPermissionOverrideHandler
        // because both WidgetPart and LayerPart are protected by the same Permission.
        protected override IEnumerable<Permission> ReplacedPermissions =>
            BasicCRUDPermissions;

        protected override Permission OverridingPermission =>
            Permissions.ManageWidgets;
    }
}