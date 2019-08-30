using System.Collections.Generic;
using Orchard.Core.Contents.Security;
using Orchard.Security.Permissions;
using Orchard.Taxonomies.Models;

namespace Orchard.Taxonomies.Security {
    public class ManageTaxonomiesPermissionOverrideHandler
        : PermissionOverrideAuthorizationEventHandler<TaxonomyPart> {
        private static Permission[] replaced = {
            Core.Contents.Permissions.DeleteContent
        };
        protected override IEnumerable<Permission> ReplacedPermissions =>
            replaced;

        protected override Permission OverridingPermission =>
            Permissions.ManageTaxonomies;
    }

    public class CreateTaxonomyPermissionOverrideHandler
        : PermissionOverrideAuthorizationEventHandler<TaxonomyPart> {
        private static Permission[] replaced = {
            Core.Contents.Permissions.CreateContent,
            Core.Contents.Permissions.EditContent,
            Core.Contents.Permissions.PublishContent
        };
        protected override IEnumerable<Permission> ReplacedPermissions =>
            replaced;

        protected override Permission OverridingPermission =>
            Permissions.CreateTaxonomy;
    }
}