using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace Orchard.Core.Navigation {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ManageMenus = new Permission { Name = "ManageMenus", Description = "Manage all menus",
            ReplaceFor = new PermissionReplaceContext {
                ReplacedPermissions = new List<Permission> {
                    Core.Contents.Permissions.CreateContent,
                    Core.Contents.Permissions.EditContent,
                    Core.Contents.Permissions.PublishContent,
                    Core.Contents.Permissions.DeleteContent
                },
                Condition = (permission, content) => {
                    string stereotype;
                    return (content != null && (content.ContentItem.TypeDefinition.Settings.TryGetValue("Stereotype", out stereotype) && stereotype == "MenuItem" || content.ContentItem.ContentType == "Menu"));
                },
                OverrideSecurable = true
            }
        };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                ManageMenus
             };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {ManageMenus}
                }
            };
        }
    }
}
