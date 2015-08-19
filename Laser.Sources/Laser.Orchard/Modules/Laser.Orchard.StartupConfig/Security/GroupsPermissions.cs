﻿using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Core.Contents.Settings;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;
using OrchardCore = Orchard.Core;
using OrchardBlogs = Orchard.Blogs;
using Orchard.Localization;
using Orchard.ContentPermissions;


namespace Laser.Orchard.StartupConfig.Security {

    [OrchardFeature("Laser.Orchard.StartupConfig.PermissionExtension")]
    public class GroupsPermissions : IPermissionProvider {

        public static readonly Permission PublishContentForOwnGroups = new Permission { Description = "Publish or unpublish for own groups", Name = "PublishGroupsContent", ImpliedBy = new[] { OrchardCore.Contents.Permissions.PublishContent}, Category = "Contents Feature" };
        public static readonly Permission EditContentForOwnGroups = new Permission { Description = "Edit for own groups", Name = "EditGroupsContent", ImpliedBy = new[] { PublishContentForOwnGroups, OrchardCore.Contents.Permissions.EditContent }, Category = "Contents Feature" };
        public static readonly Permission DeleteContentForOwnGroups = new Permission { Description = "Delete for own groups", Name = "DeleteGroupsContent", ImpliedBy = new[] { OrchardCore.Contents.Permissions.DeleteContent }, Category = "Contents Feature" };

        // Blogs
        public static readonly Permission ManageBlogsForOwnGroups = new Permission { Description = "Manage blogs for own groups", Name = "ManageGroupBlogs", ImpliedBy = new[] { OrchardBlogs.Permissions.ManageBlogs }, Category = "Orchard.Blogs Feature" };
        public static readonly Permission PublishBlogPostForOwnGroups = new Permission { Description = "Publish or unpublish blog post for own groups", Name = "PublishGroupBlogPost", ImpliedBy = new[] { OrchardBlogs.Permissions.PublishBlogPost, ManageBlogsForOwnGroups }, Category = "Orchard.Blogs Feature" };
        public static readonly Permission EditBlogPostForOwnGroups = new Permission { Description = "Edit blog posts for own groups", Name = "EditGroupBlogPost", ImpliedBy = new[] { OrchardBlogs.Permissions.EditBlogPost, PublishBlogPostForOwnGroups }, Category = "Orchard.Blogs Feature" };
        public static readonly Permission DeleteBlogPostForOwnGroups = new Permission { Description = "Delete blog post for own groups", Name = "DeleteGroupBlogPost", ImpliedBy = new[] { OrchardBlogs.Permissions.DeleteBlogPost, ManageBlogsForOwnGroups }, Category = "Orchard.Blogs Feature" };

        public virtual Feature Feature { get; set; }

        public GroupsPermissions() {
            
            if (!OrchardCore.Contents.Permissions.EditOwnContent.ImpliedBy.Contains(EditContentForOwnGroups)){
                OrchardCore.Contents.Permissions.EditOwnContent.ImpliedBy =  OrchardCore.Contents.Permissions.EditOwnContent.ImpliedBy.Concat(new[] { EditContentForOwnGroups });
            }
            if (!OrchardCore.Contents.Permissions.PublishOwnContent.ImpliedBy.Contains(PublishContentForOwnGroups)) {
                OrchardCore.Contents.Permissions.PublishOwnContent.ImpliedBy = OrchardCore.Contents.Permissions.PublishOwnContent.ImpliedBy.Concat(new[] { PublishContentForOwnGroups });
            }
            if (!OrchardCore.Contents.Permissions.DeleteOwnContent.ImpliedBy.Contains(DeleteContentForOwnGroups)) {
                OrchardCore.Contents.Permissions.DeleteOwnContent.ImpliedBy = OrchardCore.Contents.Permissions.DeleteOwnContent.ImpliedBy.Concat(new[] { DeleteContentForOwnGroups });
            }

            //Blog
            if (!OrchardBlogs.Permissions.ManageOwnBlogs.ImpliedBy.Contains(ManageBlogsForOwnGroups)) {
                OrchardBlogs.Permissions.ManageOwnBlogs.ImpliedBy = OrchardBlogs.Permissions.DeleteOwnBlogPost.ImpliedBy.Concat(new[] { ManageBlogsForOwnGroups });
            }
            if (!OrchardBlogs.Permissions.DeleteOwnBlogPost.ImpliedBy.Contains(DeleteBlogPostForOwnGroups)) {
                OrchardBlogs.Permissions.DeleteOwnBlogPost.ImpliedBy = OrchardBlogs.Permissions.DeleteOwnBlogPost.ImpliedBy.Concat(new[] { DeleteBlogPostForOwnGroups });
            }
            if (!OrchardBlogs.Permissions.PublishOwnBlogPost.ImpliedBy.Contains(PublishBlogPostForOwnGroups)) {
                OrchardBlogs.Permissions.PublishOwnBlogPost.ImpliedBy = OrchardBlogs.Permissions.PublishOwnBlogPost.ImpliedBy.Concat(new[] { PublishBlogPostForOwnGroups });
            }
            if (!OrchardBlogs.Permissions.EditOwnBlogPost.ImpliedBy.Contains(EditBlogPostForOwnGroups)) {
                OrchardBlogs.Permissions.EditOwnBlogPost.ImpliedBy = OrchardBlogs.Permissions.EditOwnBlogPost.ImpliedBy.Concat(new[] { EditBlogPostForOwnGroups });
            }
            if (!OrchardBlogs.Permissions.MetaListBlogs.ImpliedBy.Contains(EditBlogPostForOwnGroups)) {
                OrchardBlogs.Permissions.MetaListBlogs.ImpliedBy = OrchardBlogs.Permissions.MetaListOwnBlogs.ImpliedBy.Concat(new[] { DeleteBlogPostForOwnGroups, EditBlogPostForOwnGroups, PublishBlogPostForOwnGroups });
            }
        }
        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return Enumerable.Empty<PermissionStereotype>();
        }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                PublishContentForOwnGroups,
                EditContentForOwnGroups,   
                DeleteContentForOwnGroups,
                ManageBlogsForOwnGroups,
                PublishBlogPostForOwnGroups, 
                EditBlogPostForOwnGroups,
                DeleteBlogPostForOwnGroups,
    
            };
        }

    }

    //[OrchardFeature("Laser.Orchard.StartupConfig.PermissionExtension")]
    //public class DynamicGroupsPermissions : IPermissionProvider {
    //    //private static readonly Permission PublishContentForOwnGroups = new Permission { Description = "Publish or unpublish {0} for own groups", Name = "PublishGroups_{0}", ImpliedBy = new[] { GroupsPermissions.PublishContentForOwnGroups } };
    //    //private static readonly Permission EditContentForOwnGroups = new Permission { Description = "Edit {0} for own groups", Name = "EditGroups_{0}", ImpliedBy = new[] { PublishContentForOwnGroups, GroupsPermissions.EditContentForOwnGroups } };
    //    //private static readonly Permission DeleteContentForOwnGroups = new Permission { Description = "Delete {0} for own groups", Name = "DeleteGroups_{0}", ImpliedBy = new[] { GroupsPermissions.DeleteContentForOwnGroups } };

    //    private static readonly Permission PublishContentForOwnGroups = new Permission { Description = "Publish or unpublish {0} for own groups", Name = "PublishGroups_{0}", ImpliedBy = new[] { OrchardCore.Contents.Permissions.PublishContent } };
    //    private static readonly Permission EditContentForOwnGroups = new Permission { Description = "Edit {0} for own groups", Name = "EditGroups_{0}", ImpliedBy = new[] { PublishContentForOwnGroups, OrchardCore.Contents.Permissions.EditContent} };
    //    private static readonly Permission DeleteContentForOwnGroups = new Permission { Description = "Delete {0} for own groups", Name = "DeleteGroups_{0}", ImpliedBy = new[] { OrchardCore.Contents.Permissions.DeleteContent } };

    //    public static readonly Dictionary<string, Permission> DynamicPermissionTemplates = new Dictionary<string, Permission> {
    //        {OrchardCore.Contents.Permissions.PublishContent.Name, PublishContentForOwnGroups},
    //        {OrchardCore.Contents.Permissions.EditContent.Name, EditContentForOwnGroups},
    //        {OrchardCore.Contents.Permissions.DeleteContent.Name, DeleteContentForOwnGroups},
    //    };

    //    private readonly IContentDefinitionManager _contentDefinitionManager;

    //    public virtual Feature Feature { get; set; }

    //    public DynamicGroupsPermissions(IContentDefinitionManager contentDefinitionManager) {
    //        _contentDefinitionManager = contentDefinitionManager;
    //    }

    //    public IEnumerable<Permission> GetPermissions() {
    //        // manage rights only for Creatable types
    //        var creatableTypes = _contentDefinitionManager.ListTypeDefinitions()
    //            .Where(ctd => ctd.Settings.GetModel<ContentTypeSettings>().Creatable);

    //        foreach (var typeDefinition in creatableTypes) {
    //            foreach (var permissionTemplate in DynamicPermissionTemplates.Values) {
    //                yield return CreateDynamicGroupPermission(permissionTemplate, typeDefinition);
    //            }
    //        }
    //    }

    //    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
    //        return Enumerable.Empty<PermissionStereotype>();
    //    }

    //    /// <summary>
    //    /// Returns a dynamic permission for a content type, based on a global content permission template
    //    /// </summary>
    //    public static Permission ConvertToGroupPermission(Permission permission) {
    //        if (permission.Name.StartsWith("Edit_") ||
    //            permission.Name.StartsWith("EditOwn_")) {
    //            return DynamicPermissionTemplates[OrchardCore.Contents.Permissions.EditContent.Name];
    //        } else if (permission.Name.Equals("EditContent") ||
    //            permission.Name.Equals("EditOwnContent")) {
    //            return DynamicPermissionTemplates[OrchardCore.Contents.Permissions.EditContent.Name];
    //        } else if (permission.Name.StartsWith("Delete_") ||
    //                permission.Name.StartsWith("DeleteOwn_")) {
    //            return DynamicPermissionTemplates[OrchardCore.Contents.Permissions.DeleteContent.Name];
    //        } else if (permission.Name.Equals("DeleteContent") || permission.Name.Equals("DeleteOwnContent")) {
    //            return DynamicPermissionTemplates[OrchardCore.Contents.Permissions.DeleteContent.Name];
    //        } else if (permission.Name.StartsWith("Publish_") ||
    //            permission.Name.StartsWith("PublishOwn_")) {
    //            return DynamicPermissionTemplates[OrchardCore.Contents.Permissions.PublishContent.Name];
    //        } else if (permission.Name.Equals("PublishContent") ||
    //            permission.Name.Equals("PublishOwnContent")) {
    //            return DynamicPermissionTemplates[OrchardCore.Contents.Permissions.PublishContent.Name];
    //        } 

    //        //var listPermissions = permission.ImpliedBy.Select(s => s.Name).Concat(new List<string> { permission.Name });
    //        //foreach (var singlePermission in listPermissions) {
    //        //    if (DynamicPermissionTemplates.ContainsKey(singlePermission)) {
    //        //        return DynamicPermissionTemplates[singlePermission];
    //        //    }
    //        //}
    //        return null;
    //    }

    //    /// <summary>
    //    /// Generates a permission dynamically for a content type
    //    /// </summary>
    //    public static Permission CreateDynamicGroupPermission(Permission template, ContentTypeDefinition typeDefinition) {
    //        return new Permission {
    //            Name = String.Format(template.Name, typeDefinition.Name),
    //            Description = String.Format(template.Description, typeDefinition.DisplayName),
    //            Category = typeDefinition.DisplayName,
    //            ImpliedBy = (template.ImpliedBy ?? new Permission[0]).Select(t => OrchardCore.Contents.DynamicPermissions.CreateDynamicPermission(t, typeDefinition))
    //        };
    //    }

    //}

}