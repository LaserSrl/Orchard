using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;
using System.Collections.Generic;

namespace Laser.Orchard.CommunicationGateway {

    public class Permissions : IPermissionProvider {
        public static readonly Permission ManageCampaigns = new Permission { Description = "Manage Comunication Campaigns", Name = "ManageCampaigns"};
        public static readonly Permission ManageCommunicationAdv = new Permission { Description = "Manage Comunication Messages", Name = "ManageCommunicationAdv", ImpliedBy = new[] {ManageCampaigns} };
        public static readonly Permission ManageContact = new Permission { Description = "Manage Comunication Contact", Name = "ManageContact", ImpliedBy = new[] { ManageCampaigns } };
        public static readonly Permission ShowMenuCommunication = new Permission { Description = "Show Menu Communication", Name = "ShowMenuCommunication", ImpliedBy = new[] { ManageCampaigns, ManageCommunicationAdv, ManageContact } };


        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                ManageCampaigns,
                ManageCommunicationAdv,
                ManageContact,
                ShowMenuCommunication
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                 Permissions = new[] {ManageCampaigns,ManageCommunicationAdv,ManageContact,ShowMenuCommunication}
                },
                new PermissionStereotype {
                    Name = "Editor",
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