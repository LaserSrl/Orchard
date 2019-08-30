using System.Collections.Generic;
using Orchard.Blogs.Models;
using Orchard.Core.Contents.Security;
using Orchard.Security.Permissions;

namespace Orchard.Blogs.Security {
    public class PublishBlogPostPermissionOverrideHandler
        : PermissionOverrideAuthorizationEventHandler<BlogPostPart> {
        private static Permission[] replaced = {
            Core.Contents.Permissions.PublishContent
        };
        protected override IEnumerable<Permission> ReplacedPermissions =>
            replaced;

        protected override Permission OverridingPermission =>
            Permissions.PublishBlogPost;
    }
    public class EditBlogPostPermissionOverrideHandler
        : PermissionOverrideAuthorizationEventHandler<BlogPostPart> {
        private static Permission[] replaced = {
            Core.Contents.Permissions.CreateContent,
            Core.Contents.Permissions.EditContent
        };
        protected override IEnumerable<Permission> ReplacedPermissions =>
            replaced;

        protected override Permission OverridingPermission =>
            Permissions.EditBlogPost;
    }
    public class DeleteBlogPostPermissionOverrideHandler
        : PermissionOverrideAuthorizationEventHandler<BlogPostPart> {
        private static Permission[] replaced = {
            Core.Contents.Permissions.DeleteContent
        };
        protected override IEnumerable<Permission> ReplacedPermissions =>
            replaced;

        protected override Permission OverridingPermission =>
            Permissions.DeleteBlogPost;
    }
}