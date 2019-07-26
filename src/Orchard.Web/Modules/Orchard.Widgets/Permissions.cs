using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;
using Orchard.Widgets.Models;

namespace Orchard.Widgets {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ManageWidgets = new Permission { Description = "Managing Widgets", Name = "ManageWidgets",
            ReplaceFor = new PermissionReplaceContext {
                ReplacedPermissions = new List<Permission> {
                    Core.Contents.Permissions.CreateContent,
                    Core.Contents.Permissions.EditContent,
                    Core.Contents.Permissions.PublishContent,
                    Core.Contents.Permissions.DeleteContent
                },
                Condition = (permission, content) => {
                    return content != null && content.Is<WidgetPart>();
                },
                OverrideSecurable = false
            }
        };

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
    }
}