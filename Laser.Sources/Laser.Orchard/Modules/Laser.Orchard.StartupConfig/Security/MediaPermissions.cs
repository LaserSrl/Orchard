using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OrchardCore = Orchard.Core;
using OrchardMedia = Orchard.MediaLibrary;

namespace Laser.Orchard.StartupConfig.Security {
    public class MediaPermissions : IPermissionProvider {
        public static readonly Permission DeleteMedia = new Permission { Description = "Delete Media", Name = "DeleteMedia",ImpliedBy = new[] { OrchardMedia.Permissions.ManageMediaContent }, Category = "Orchard.MediaLibrary Feature" };
        public static readonly Permission EditMedia = new Permission { Description = "Edit Media", Name = "EditMedia", ImpliedBy = new[] { MediaPermissions.DeleteMedia, OrchardMedia.Permissions.ManageMediaContent }, Category = "Orchard.MediaLibrary Feature" };
        public static readonly Permission InsertMedia = new Permission { Description = "Insert Media", Name = "InsertMedia", ImpliedBy = new[] { MediaPermissions.DeleteMedia, MediaPermissions.EditMedia, OrchardMedia.Permissions.ManageMediaContent }, Category = "Orchard.MediaLibrary Feature" };
        public static readonly Permission ViewMedia = new Permission { Description = "View Media", Name = "ViewMedia", ImpliedBy = new[] { MediaPermissions.DeleteMedia, MediaPermissions.EditMedia,MediaPermissions.InsertMedia, OrchardMedia.Permissions.ManageMediaContent }, Category = "Orchard.MediaLibrary Feature" };

        //public MediaPermissions() { 
        //OrchardMedia.Permissions.ManageMediaContent.ImpliedBy

        //         if (!OrchardMedia.Permissions.ManageMediaContent.ImpliedBy.Contains(EditContentForOwnGroups)){
        //        OrchardCore.Contents.Permissions.EditOwnContent.ImpliedBy =  OrchardCore.Contents.Permissions.EditOwnContent.ImpliedBy.Concat(new[] { EditContentForOwnGroups });
        //    }
        //}
  
    //    public static readonly Permission FolderEditMedia = new Permission { Description = "Folder Edit Media", Name = "FolderEditMedia", Category = "Orchard.MediaLibrary Feature" };

        public virtual Feature Feature { get; set; }
        public MediaPermissions() {
           //if (!OrchardMedia.Permissions.ManageMediaContent.ImpliedBy.Contains(FolderEditMedia)) {
            //    OrchardMedia.Permissions.ManageMediaContent.ImpliedBy = OrchardMedia.Permissions.ManageMediaContent.ImpliedBy.Concat(new[] { FolderEditMedia });
           //}
        }
        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return Enumerable.Empty<PermissionStereotype>();
        }
        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                 ViewMedia,
                 EditMedia,
                 DeleteMedia,
                 InsertMedia
                // FolderEditMedia
            };
        }
    }
}