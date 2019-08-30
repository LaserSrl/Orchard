using System.Collections.Generic;
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
        private static Permission[] replaced = {
            Core.Contents.Permissions.CreateContent
        };
        protected override IEnumerable<Permission> ReplacedPermissions =>
            replaced;

        protected override Permission OverridingPermission =>
            Permissions.CreateTerm;
    }
    public class EditTermPermissionOverrideHandler
        : TermPermissionOverrideHandler {
        private static Permission[] replaced = {
            Core.Contents.Permissions.EditContent
        };
        protected override IEnumerable<Permission> ReplacedPermissions =>
            replaced;

        protected override Permission OverridingPermission =>
            Permissions.EditTerm;
    }
    public class DeleteTermPermissionOverrideHandler
        : TermPermissionOverrideHandler {
        private static Permission[] replaced = {
            Core.Contents.Permissions.DeleteContent,
        };
        protected override IEnumerable<Permission> ReplacedPermissions =>
            replaced;

        protected override Permission OverridingPermission =>
            Permissions.DeleteTerm;
    }
}