using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;
using Orchard.Taxonomies.Models;

namespace Orchard.Taxonomies {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ManageTaxonomies = new Permission {
            Description = "Manage taxonomies",
            Name = "ManageTaxonomies",
            ReplaceFor = new PermissionReplaceContext {
                ReplacedPermissions = new List<Permission> {
                    Orchard.Core.Contents.Permissions.DeleteContent
                },
                Condition = (permission, content) => content != null && content.Is<TaxonomyPart>(),
                OverrideSecurable = false
            }

        };
        public static readonly Permission CreateTaxonomy = new Permission {
            Description = "Create taxonomy",
            Name = "CreateTaxonomy",
            ImpliedBy = new[] { ManageTaxonomies },
            ReplaceFor = new PermissionReplaceContext {
                ReplacedPermissions = new List<Permission> {
                    Orchard.Core.Contents.Permissions.CreateContent,
                    Orchard.Core.Contents.Permissions.EditContent,
                    Orchard.Core.Contents.Permissions.PublishContent
                },
                Condition = (permission, content) => content != null && content.Is<TaxonomyPart>(),
                OverrideSecurable = false
            }
        };
        public static readonly Permission ManageTerms = new Permission { Description = "Manage terms", Name = "ManageTerms", ImpliedBy = new[] { CreateTaxonomy } };
        public static readonly Permission MergeTerms = new Permission { Description = "Merge terms", Name = "MergeTerms", ImpliedBy = new[] { ManageTerms } };
        public static readonly Permission CreateTerm = new Permission {
            Description = "Create term",
            Name = "CreateTerm",
            ImpliedBy = new[] { ManageTerms, MergeTerms },
            ReplaceFor = new PermissionReplaceContext {
                ReplacedPermissions = new List<Permission> {
                    Orchard.Core.Contents.Permissions.CreateContent
                },
                Condition = (permission, content) => content != null && content.Is<TermPart>(),
                OverrideSecurable = false
            }
        };
        public static readonly Permission EditTerm = new Permission {
            Description = "Edit term",
            Name = "EditTerm",
            ImpliedBy = new[] { ManageTerms, MergeTerms },
            ReplaceFor = new PermissionReplaceContext {
                ReplacedPermissions = new List<Permission> {
                    Orchard.Core.Contents.Permissions.EditContent
                },
                Condition = (permission, content) => content != null && content.Is<TermPart>(),
                OverrideSecurable = false
            }
        };
        public static readonly Permission DeleteTerm = new Permission {
            Description = "Delete term",
            Name = "DeleteTerm",
            ImpliedBy = new[] { ManageTerms, MergeTerms },
            ReplaceFor = new PermissionReplaceContext {
                ReplacedPermissions = new List<Permission> {
                    Orchard.Core.Contents.Permissions.DeleteContent
                },
                Condition = (permission, content) => content != null && content.Is<TermPart>(),
                OverrideSecurable = false
            }
        };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                ManageTaxonomies,
                CreateTaxonomy,
                ManageTerms,
                MergeTerms,
                CreateTerm,
                EditTerm,
                DeleteTerm
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {ManageTaxonomies}
                },
                new PermissionStereotype {
                    Name = "Editor",
                    Permissions = new[] {ManageTaxonomies}
                },
                new PermissionStereotype {
                    Name = "Moderator",
                    Permissions = new[] {ManageTaxonomies}
                },
                new PermissionStereotype {
                    Name = "Author",
                    Permissions = new[] {CreateTaxonomy}
                },
                new PermissionStereotype {
                    Name = "Contributor",
                    Permissions = new Permission[0]
                },
            };
        }
    }
}

