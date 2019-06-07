using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;
using Orchard.Widgets.Models;

namespace Orchard.Widgets {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ManageWidgets = new Permission { Description = "Managing Widgets", Name = "ManageWidgets" };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                ManageWidgets,
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {ManageWidgets}
                },
            };
        }

        private static bool OverridePermissions(Permission sourcePermission, IContent content) {
            if (content != null && content.Is<WidgetPart>()) {
                if (sourcePermission == Orchard.Core.Contents.Permissions.CreateContent) {
                    return true;
                }
                else if (sourcePermission == Orchard.Core.Contents.Permissions.EditContent) {
                    return true;
                }
                else if (sourcePermission == Orchard.Core.Contents.Permissions.PublishContent) {
                    return true;
                }
                else if (sourcePermission == Orchard.Core.Contents.Permissions.DeleteContent) {
                    return true;
                }
            }
            return false;

        }

    }
}