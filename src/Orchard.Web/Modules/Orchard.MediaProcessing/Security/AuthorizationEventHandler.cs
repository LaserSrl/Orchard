using Orchard.ContentManagement;
using Orchard.MediaProcessing.Models;
using Orchard.Security;
using Orchard.Security.Permissions;

namespace Orchard.MediaProcessing.Security {
    public class AuthorizationEventHandler : IAuthorizationServiceEventHandler {
        public void Checking(CheckAccessContext context) { }
        public void Complete(CheckAccessContext context) { }

        public void Adjust(CheckAccessContext context) {
            Permission permission = context.Permission;
            if (context.Content.Is<ImageProfilePart>()) {
                if (context.Permission == Core.Contents.Permissions.CreateContent) {
                    permission = StandardPermissions.SiteOwner;
                }
                else if (context.Permission == Core.Contents.Permissions.EditContent || context.Permission == Core.Contents.Permissions.EditOwnContent) {
                    permission = StandardPermissions.SiteOwner;
                }
                else if (context.Permission == Core.Contents.Permissions.PublishContent || context.Permission == Core.Contents.Permissions.PublishOwnContent) {
                    permission = StandardPermissions.SiteOwner;
                }
                else if (context.Permission == Core.Contents.Permissions.DeleteContent || context.Permission == Core.Contents.Permissions.DeleteOwnContent) {
                    permission = StandardPermissions.SiteOwner;
                }
            }
            if (permission != context.Permission) {
                context.Granted = false; //Force granted to false so next adjust iteration will check against the new permission starting from an unauthorized condition
                context.Permission = permission;
                context.Adjusted = true;
            }
        }
    }
}
