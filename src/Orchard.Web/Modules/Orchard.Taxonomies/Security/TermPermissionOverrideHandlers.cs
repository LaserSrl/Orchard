﻿using System.Collections.Generic;
using Orchard.Core.Contents.Security;
using Orchard.Security.Permissions;
using Orchard.Taxonomies.Models;

namespace Orchard.Taxonomies.Security {

    public abstract class TermPermissionOverrideHandler
        : PermissionOverrideAuthorizationEventHandler<TermPart> {
        protected override bool ConsiderSecurableSetting => true;
    }

    public class CreateTermPermissionOverrideHandler
        : TermPermissionOverrideHandler {
        protected override IEnumerable<Permission> ReplacedPermissions =>
            new List<Permission> {
                Orchard.Core.Contents.Permissions.CreateContent
            };

        protected override Permission OverridingPermission =>
            Permissions.CreateTerm;
    }
    public class EditTermPermissionOverrideHandler
        : TermPermissionOverrideHandler {
        protected override IEnumerable<Permission> ReplacedPermissions =>
            new List<Permission> {
                Orchard.Core.Contents.Permissions.EditContent
            };

        protected override Permission OverridingPermission =>
            Permissions.EditTerm;
    }
    public class DeleteTermPermissionOverrideHandler
        : TermPermissionOverrideHandler {
        protected override IEnumerable<Permission> ReplacedPermissions =>
            new List<Permission> {
                Orchard.Core.Contents.Permissions.DeleteContent
            };

        protected override Permission OverridingPermission =>
            Permissions.DeleteTerm;
    }
}