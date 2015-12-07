using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Security.Permissions;
using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;


namespace Laser.Orchard.CommunicationGateway {
    public class Permissions: IPermissionProvider{
        public static readonly Permission ManageCampaigns = new Permission { Description = "Manage Campaigns", Name = "ManageCampaigns" };
        public static readonly Permission ManageCommunicationAdv = new Permission { Description = "Manage Comunication Messages", Name = "ManageCommunicationAdv" };
    
        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                ManageCampaigns,
                ManageCommunicationAdv
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                 Permissions = new[] {ManageCampaigns,ManageCommunicationAdv}
                },
                new PermissionStereotype {
                    Name = "Editor",
                    Permissions = new[] {ManageCampaigns,ManageCommunicationAdv}
                },
                new PermissionStereotype {
                    Name = "Moderator",
                  },
                new PermissionStereotype {
                    Name = "Author",
                },
                new PermissionStereotype {
                    Name = "Contributor",
                },
            };
        }
    }
}