using Orchard.ContentManagement;
using Orchard.Security;
using Orchard.Security.Permissions;
using Orchard.ContentManagement.Aspects;

namespace Orchard.Dashboards.Security {
    public class AuthorizationEventHandler : IAuthorizationServiceEventHandler {
        public void Checking(CheckAccessContext context) { }
        public void Complete(CheckAccessContext context) { }

        public void Adjust(CheckAccessContext context) {
            Permission permission = context.Permission;
            if (context.Content != null && context.Content.ContentItem.ContentType == "Dashboard") {
                if (context.Permission == Core.Contents.Permissions.CreateContent) {
                    permission = Permissions.ManageDashboards;
                }
                else if (context.Permission == Core.Contents.Permissions.EditContent || context.Permission == Core.Contents.Permissions.EditOwnContent) {
                    permission = Permissions.ManageDashboards;
                }
                else if (context.Permission == Core.Contents.Permissions.PublishContent || context.Permission == Core.Contents.Permissions.PublishOwnContent) {
                    permission = Permissions.ManageDashboards;
                }
                else if (context.Permission == Core.Contents.Permissions.DeleteContent || context.Permission == Core.Contents.Permissions.DeleteOwnContent) {
                    permission = Permissions.ManageDashboards;
                }
            }
            if (permission != context.Permission) {
                context.Granted = false; //Force granted to false so next adjust iteration will check against the new permission starting from an unauthorized condition
                context.Permission = permission;
                context.Adjusted = true;
            }
            if (OwnerVariationExists(context.Permission) && HasOwnership(context.User, context.Content)) {
                context.Permission = GetOwnerVariation(context.Permission);
                context.Adjusted = true;
            }
        }
        private static bool HasOwnership(IUser user, IContent content) {
            if (user == null || content == null)
                return false;

            var common = content.As<ICommonPart>();
            if (common == null || common.Owner == null)
                return false;

            return user.Id == common.Owner.Id;
        }

        private static bool OwnerVariationExists(Permission permission) {
            return GetOwnerVariation(permission) != null;
        }

        private static Permission GetOwnerVariation(Permission permission) {
            //Dashboard
            if (permission.Name == Permissions.ManageDashboards.Name)
                return Permissions.ManageOwnDashboard;
            return null;
        }


    }
}
