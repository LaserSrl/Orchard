using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Security;
using Orchard.Security.Permissions;

namespace Orchard.Blogs.Security {
    public class BlogAuthorizationEventHandler : IAuthorizationServiceEventHandler {
        public void Checking(CheckAccessContext context) { }
        public void Complete(CheckAccessContext context) { }

        public void Adjust(CheckAccessContext context) {
            if (context.Content.Is<ICommonPart>()) {

                if (context.Content.ContentItem.ContentType == "BlogPost" &&
                    BlogPostVariationExists(context.Permission)) {
                    context.Granted = false; //Force granted to false so next adjust iteration will check against the new permission starting from an unauthorized condition
                    context.Adjusted = true;
                    context.Permission = GetBlogPostVariation(context.Permission);
                }
                else if (context.Content.ContentItem.ContentType == "Blog" &&
                    BlogVariationExists(context.Permission)) {
                    context.Granted = false; //Force granted to false so next adjust iteration will check against the new permission starting from an unauthorized condition
                    context.Adjusted = true;
                    context.Permission = GetBlogVariation(context.Permission);
                }
                if (OwnerVariationExists(context.Permission) &&
                    HasOwnership(context.User, context.Content)) {
                    context.Granted = false; //Force granted to false so next adjust iteration will check against the new permission starting from an unauthorized condition
                    context.Adjusted = true;
                    context.Permission = GetOwnerVariation(context.Permission);
                }
            }
        }

        private static bool HasOwnership(IUser user, IContent content) {
            if (user == null || content == null)
                return false;

            if (HasOwnershipOnContainer(user, content)) {
                return true;
            }

            var common = content.As<ICommonPart>();
            if (common == null || common.Owner == null)
                return false;

            return user.Id == common.Owner.Id;
        }

        private static bool HasOwnershipOnContainer(IUser user, IContent content) {
            if (user == null || content == null)
                return false;

            var common = content.As<ICommonPart>();
            if (common == null || common.Container == null)
                return false;

            common = common.Container.As<ICommonPart>();
            if (common == null || common.Container == null)
                return false;

            return user.Id == common.Owner.Id;
        }

        private static bool OwnerVariationExists(Permission permission) {
            return GetOwnerVariation(permission) != null;
        }

        private static Permission GetOwnerVariation(Permission permission) {
            //BlogPost
            if (permission.Name == Permissions.PublishBlogPost.Name)
                return Permissions.PublishOwnBlogPost;
            if (permission.Name == Permissions.EditBlogPost.Name)
                return Permissions.EditOwnBlogPost;
            if (permission.Name == Permissions.DeleteBlogPost.Name)
                return Permissions.DeleteOwnBlogPost;
            if (permission.Name == Core.Contents.Permissions.ViewContent.Name)
                return Core.Contents.Permissions.ViewOwnContent;
            if (permission.Name == Permissions.MetaListBlogs.Name)
                return Permissions.MetaListOwnBlogs;
            //Blog
            if (permission.Name == Permissions.ManageBlogs.Name)
                return Permissions.ManageOwnBlogs;
            return null;
        }

        private static bool BlogPostVariationExists(Permission permission) {
            return GetBlogPostVariation(permission) != null;
        }
        private static bool BlogVariationExists(Permission permission) {
            return GetBlogVariation(permission) != null;
        }
        private static Permission GetBlogPostVariation(Permission permission) {
            if (permission.Name == Orchard.Core.Contents.Permissions.CreateContent.Name)
                return Permissions.EditBlogPost;
            if (permission.Name == Orchard.Core.Contents.Permissions.PublishContent.Name)
                return Permissions.PublishBlogPost;
            if (permission.Name == Orchard.Core.Contents.Permissions.PublishOwnContent.Name)
                return Permissions.PublishOwnBlogPost;
            if (permission.Name == Orchard.Core.Contents.Permissions.EditContent.Name)
                return Permissions.EditBlogPost;
            if (permission.Name == Orchard.Core.Contents.Permissions.EditOwnContent.Name)
                return Permissions.EditOwnBlogPost;
            if (permission.Name == Orchard.Core.Contents.Permissions.DeleteContent.Name)
                return Permissions.DeleteBlogPost;
            if (permission.Name == Orchard.Core.Contents.Permissions.DeleteOwnContent.Name)
                return Permissions.DeleteOwnBlogPost;

            return null;
        }

        private static Permission GetBlogVariation(Permission permission) {
            if (permission.Name == Orchard.Core.Contents.Permissions.CreateContent.Name)
                return Permissions.ManageBlogs;
            if (permission.Name == Orchard.Core.Contents.Permissions.PublishContent.Name)
                return Permissions.ManageBlogs;
            if (permission.Name == Orchard.Core.Contents.Permissions.PublishOwnContent.Name)
                return Permissions.ManageBlogs;
            if (permission.Name == Orchard.Core.Contents.Permissions.EditContent.Name)
                return Permissions.ManageBlogs;
            if (permission.Name == Orchard.Core.Contents.Permissions.EditOwnContent.Name)
                return Permissions.ManageBlogs;
            if (permission.Name == Orchard.Core.Contents.Permissions.DeleteContent.Name)
                return Permissions.ManageBlogs;
            if (permission.Name == Orchard.Core.Contents.Permissions.DeleteOwnContent.Name)
                return Permissions.ManageBlogs;

            return null;
        }
    }
}