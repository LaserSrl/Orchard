using System.Collections.Generic;
using Orchard.Core.Contents.Security;
using Orchard.Security.Permissions;
using Orchard.Taxonomies.Models;

namespace Orchard.Taxonomies.Security {
    public class ManageTaxonomiesPermissionOverrideHandler
        : PermissionOverrideAuthorizationEventHandler<TaxonomyPart> {
        protected override IEnumerable<Permission> ReplacedPermissions =>
            new List<Permission> {
                Orchard.Core.Contents.Permissions.DeleteContent
            };

        protected override Permission OverridingPermission =>
            Permissions.ManageTaxonomies;
    }

    public class CreateTaxonomyPermissionOverrideHandler
        : PermissionOverrideAuthorizationEventHandler<TaxonomyPart> {
        protected override IEnumerable<Permission> ReplacedPermissions =>
            new List<Permission> {
                Orchard.Core.Contents.Permissions.CreateContent,
                Orchard.Core.Contents.Permissions.EditContent,
                Orchard.Core.Contents.Permissions.PublishContent
            };

        protected override Permission OverridingPermission =>
            Permissions.CreateTaxonomy;
    }
}