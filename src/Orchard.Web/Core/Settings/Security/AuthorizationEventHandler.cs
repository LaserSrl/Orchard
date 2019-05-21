using Orchard.ContentManagement;
using Orchard.Core.Navigation.Models;
using Orchard.Security;
using Orchard.Security.Permissions;

namespace  Orchard.Core.Settings.Security {
    public class AuthorizationEventHandler : IAuthorizationServiceEventHandler {
        private readonly IContentManager _contentManager;

        public AuthorizationEventHandler(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public void Checking(CheckAccessContext context) { }
        public void Complete(CheckAccessContext context) { }

        public void Adjust(CheckAccessContext context) {
            Permission permission = context.Permission;
            if (context.Content != null && context.Content.ContentItem.ContentType == "Site") {
                if (context.Permission == Core.Contents.Permissions.CreateContent) {
                    permission = Permissions.ManageSettings;
                }
                else if (context.Permission == Core.Contents.Permissions.EditContent || context.Permission == Core.Contents.Permissions.EditOwnContent) {
                    permission = Permissions.ManageSettings;
                }
                else if (context.Permission == Core.Contents.Permissions.PublishContent || context.Permission == Core.Contents.Permissions.PublishOwnContent) {
                    permission = Permissions.ManageSettings;
                }
                else if (context.Permission == Core.Contents.Permissions.DeleteContent || context.Permission == Core.Contents.Permissions.DeleteOwnContent) {
                    permission = Permissions.ManageSettings;
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