using System.Collections.Generic;
using Orchard.Blogs.Models;
using Orchard.Core.Contents.Security;
using Orchard.Security.Permissions;

namespace Orchard.Blogs.Security {
    public class PublishBlogPostPermissionOverrideHandler
        : PermissionOverrideAuthorizationEventHandler<BlogPostPart> {
        protected override IEnumerable<Permission> ReplacedPermissions =>
            new List<Permission>() {
                Core.Contents.Permissions.PublishContent
            };

        protected override Permission OverridingPermission =>
            Permissions.PublishBlogPost;
    }
    public class EditBlogPostPermissionOverrideHandler
        : PermissionOverrideAuthorizationEventHandler<BlogPostPart> {
        protected override IEnumerable<Permission> ReplacedPermissions =>
            new List<Permission>() {
                Core.Contents.Permissions.CreateContent,
                Core.Contents.Permissions.EditContent
            };

        protected override Permission OverridingPermission =>
            Permissions.EditBlogPost;
    }
    public class DeleteBlogPostPermissionOverrideHandler
        : PermissionOverrideAuthorizationEventHandler<BlogPostPart> {
        protected override IEnumerable<Permission> ReplacedPermissions =>
            new List<Permission>() {
                Core.Contents.Permissions.DeleteContent,
            };

        protected override Permission OverridingPermission =>
            Permissions.DeleteBlogPost;
    }
}