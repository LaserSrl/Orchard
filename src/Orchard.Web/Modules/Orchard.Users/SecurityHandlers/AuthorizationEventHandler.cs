using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Security;
using Orchard.Security.Permissions;
using Orchard.Users.Models;

namespace Orchard.Users.SecurityHandlers {
    public class AuthorizationEventHandler : IAuthorizationServiceEventHandler {
        public void Checking(CheckAccessContext context) { }
        public void Complete(CheckAccessContext context) { }

        public void Adjust(CheckAccessContext context) {
            Permission permission = context.Permission;
            if (context.Content.Is<UserPart>()) {
                if (context.Permission == Core.Contents.Permissions.CreateContent) {
                    permission = Permissions.ManageUsers;
                }
                else if (context.Permission == Core.Contents.Permissions.EditContent || context.Permission == Core.Contents.Permissions.EditOwnContent) {
                    permission = Permissions.ManageUsers;
                }
                else if (context.Permission == Core.Contents.Permissions.PublishContent || context.Permission == Core.Contents.Permissions.PublishOwnContent) {
                    permission = Permissions.ManageUsers;
                }
                else if (context.Permission == Core.Contents.Permissions.DeleteContent || context.Permission == Core.Contents.Permissions.DeleteOwnContent) {
                    permission = Permissions.ManageUsers;
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
