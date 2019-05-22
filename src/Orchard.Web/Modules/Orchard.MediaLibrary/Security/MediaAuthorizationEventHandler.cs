using Orchard.ContentManagement;
using Orchard.MediaLibrary.Models;
using Orchard.MediaLibrary.Services;
using Orchard.Security;
using Orchard.Security.Permissions;

namespace Orchard.MediaLibrary.Security {
    public class MediaAuthorizationEventHandler : IAuthorizationServiceEventHandler {
        private readonly IAuthorizer _authorizer;
        private readonly IMediaLibraryService _mediaLibraryService;

        public MediaAuthorizationEventHandler(
            IAuthorizer authorizer,
            IMediaLibraryService mediaLibraryService) {
            _authorizer = authorizer;
            _mediaLibraryService = mediaLibraryService;
        }

        public void Checking(CheckAccessContext context) { }
        public void Complete(CheckAccessContext context) { }

        public void Adjust(CheckAccessContext context) {
            var mediaPart = context.Content.As<MediaPart>();
            if (mediaPart != null) {
                Permission permission = context.Permission;
                var typeDefinition = context.Content.ContentItem.TypeDefinition;
                if (context.Permission == Core.Contents.Permissions.CreateContent) {
                    permission = Permissions.EditMediaContent;
                }
                else if (context.Permission == Core.Contents.Permissions.EditContent || context.Permission == Core.Contents.Permissions.EditOwnContent) {
                    permission = Permissions.EditMediaContent;
                }
                else if (context.Permission == Core.Contents.Permissions.PublishContent || context.Permission == Core.Contents.Permissions.PublishOwnContent) {
                    permission = Permissions.EditMediaContent;
                }
                else if (context.Permission == Core.Contents.Permissions.DeleteContent || context.Permission == Core.Contents.Permissions.DeleteOwnContent) {
                    permission = Permissions.DeleteMediaContent;
                }
                if (permission != context.Permission) {
                    context.Granted = false; //Force granted to false so next adjust iteration will check against the new permission starting from an unauthorized condition
                    context.Permission = permission;
                    context.Adjusted = true;
                }
                context.Granted = _mediaLibraryService.CheckMediaFolderPermission(context.Permission, mediaPart.FolderPath);
            }
        }
    }
}