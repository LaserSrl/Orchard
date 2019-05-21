using Orchard.ContentManagement;
using Orchard.Core.Navigation.Models;
using Orchard.Security;
using Orchard.Security.Permissions;

namespace  Orchard.Core.Navigation.Security {
    public class AuthorizationEventHandler : IAuthorizationServiceEventHandler {
        private readonly IContentManager _contentManager;

        public AuthorizationEventHandler(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public void Checking(CheckAccessContext context) { }
        public void Complete(CheckAccessContext context) { }

        public void Adjust(CheckAccessContext context) {
            Permission permission = context.Permission;
            string stereotype;
            if (context.Content != null && (context.Content.ContentItem.TypeDefinition.Settings.TryGetValue("Stereotype", out stereotype) && stereotype == "MenuItem" ||  context.Content.ContentItem.ContentType == "Menu")) {
                if (context.Permission == Core.Contents.Permissions.CreateContent) {
                    permission = Permissions.ManageMenus;
                }
                else if (context.Permission == Core.Contents.Permissions.EditContent || context.Permission == Core.Contents.Permissions.EditOwnContent) {
                    permission = Permissions.ManageMenus;
                }
                else if (context.Permission == Core.Contents.Permissions.PublishContent || context.Permission == Core.Contents.Permissions.PublishOwnContent) {
                    permission = Permissions.ManageMenus;
                }
                else if (context.Permission == Core.Contents.Permissions.DeleteContent || context.Permission == Core.Contents.Permissions.DeleteOwnContent) {
                    permission = Permissions.ManageMenus;
                }
            }
            if (permission != context.Permission) {
                context.Granted = false; //Force granted to false so next adjust iteration will check against the new permission starting from an unauthorized condition
                context.Permission = permission;
                context.Adjusted = true;
            }
            if (!context.Granted && context.Permission.Name == Permissions.ManageMenus.Name && context.Content != null) {
                
                var menuAsContentItem = context.Content.As<ContentItem>();
                if (menuAsContentItem == null || menuAsContentItem.Id <= 0) {
                    return;
                }

                context.Adjusted = true;
                context.Permission = DynamicPermissions.CreateMenuPermission(menuAsContentItem, _contentManager);
            }
        }
    }
}