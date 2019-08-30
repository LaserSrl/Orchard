using System.Collections.Generic;
using Orchard.Core.Contents.Security;
using Orchard.CustomForms.Models;
using Orchard.Security.Permissions;

namespace Orchard.CustomForms.Security {
    public class ManageFormsPermissionOverrideHandler
        : PermissionOverrideAuthorizationEventHandler<CustomFormPart> {
        protected override IEnumerable<Permission> ReplacedPermissions =>
            BasicCRUDPermissions;

        protected override Permission OverridingPermission =>
            Permissions.ManageForms;
    }
}