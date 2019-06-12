using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace Orchard.Core.Settings {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ManageSettings = new Permission { Description = "Manage Settings", Name = "ManageSettings",
            ReplaceFor = new PermissionReplaceContext {
                ReplacedPermissions = new List<Permission>() {
                    Core.Contents.Permissions.CreateContent,
                    Core.Contents.Permissions.EditContent,
                    Core.Contents.Permissions.PublishContent,
                    Core.Contents.Permissions.DeleteContent,
                },
                Condition = (permission, content) => (content != null && content.ContentItem.ContentType == "Site"),
                OverrideSecurable = true
            }
        };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                ManageSettings
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {ManageSettings}
                },
            };
        }

    }
}