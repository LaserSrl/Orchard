using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Core.Contents.Settings;
using Orchard.Security;
using Orchard.Security.Permissions;

namespace Orchard.Core.Contents.Security {
    /// <summary>
    /// Provides a base class for those situations when a specific permission should "override" more
    /// generic ones, e.g. when a custom "ManageX" permission should be verified rather than EditContent
    /// and other similar default permissions.
    /// The idea is for each implementation of this to be returning a single overriding Permission.
    /// </summary>
    public abstract class PermissionOverrideAuthorizationEventHandler : IAuthorizationServiceEventHandler {
        /// <summary>
        /// The IEnumerable of Permissions that should be overridden. This handler does
        /// anything at all only if the permission being tested is in this IEnumerable.
        /// </summary>
        protected abstract IEnumerable<Permission> ReplacedPermissions { get; }
        /// <summary>
        /// Compute the specific permission that should be used rather than the one
        /// already specified from the context.
        /// </summary>
        /// <param name="context">The context object for the authorization being tested.</param>
        /// <returns>The specific permission we should be using. If the return value is
        /// null, the permission currently in the context will not be overriden.</returns>
        /// <remarks>This method is called after verifying that the handler should override
        /// the current context.Permission by checking that it is contained in ReplacedPermissions.
        /// If for all Permissions in the IEnumerable the method should return the same override,
        /// implementations don't need to check again the Permission under test.</remarks>
        protected virtual Permission EvaluateOverride(CheckAccessContext context) {
            if (ShouldOverride(context)) {
                return OverridingPermission;
            }
            return null;
        }
        /// <summary>
        /// This is the permission that we will be using to override the one we are currently
        /// testing the authorization for.
        /// </summary>
        protected abstract Permission OverridingPermission { get; }
        /// <summary>
        /// Compute whether the handler should be replacing the Permission against which we are
        /// testing the authorization with the override.
        /// </summary>
        /// <param name="context">The context object for the authorization being tested.</param>
        /// <returns>A boolean that tells the handler whether to go ahead and try to override the
        /// permission.</returns>
        /// <remarks>Specific implementations of this method should generally also use the
        /// result from the base class with base.ShouldOverride(context).</remarks>
        protected virtual bool ShouldOverride(CheckAccessContext context) {
            return ReplacedPermissions.Contains(context.Permission)
                && (ConsiderSecurableSetting && !ContentIsSecurable(context));
        }
        /// <summary>
        /// Utility method that verifies whether the ContentType of the ContentItem (if any) in
        /// the context against which we are checking authorization is set as Securable.
        /// </summary>
        /// <param name="context">The context object for the authorization being tested.</param>
        /// <returns>The value of the Securable setting for the ContentType.</returns>
        protected static bool ContentIsSecurable(CheckAccessContext context) {
            if (context.Content != null && context.Content.ContentItem != null) {
                var typeDefinition = context.Content.ContentItem.TypeDefinition;
                return typeDefinition.Settings.GetModel<ContentTypeSettings>().Securable;
            }
            return false;
        }
        /// <summary>
        /// boolean property that tells whether the Securable flag for the content
        /// should be considered. When this is true, if the content is securable
        /// we should not override the permission.
        /// </summary>
        protected virtual bool ConsiderSecurableSetting => false;

        public void Checking(CheckAccessContext context) {
            // This method updates context.Permissions:
            // We will check whether context.Permission should be replaced
            // by a different Permission. If that is the case, we will remove
            // context.Permission from context.Permissions, and add the specific
            // Permission we found.
            var newPermission = EvaluateOverride(context);
            if (newPermission != null && newPermission != context.Permission) {
                context.Permissions.Remove(context.Permission);
                context.Permissions.Add(newPermission);
                context.Permissions = context.Permissions.Distinct().ToList();
            }
        }

        public void Adjust(CheckAccessContext context) { }
        public void Complete(CheckAccessContext context) { }
    }
    /// <summary>
    /// "Partial" implementation of PermissionOverrideAuthorizationEventHandler that provides
    /// a test done on the fact that the content for which we are checking an authorization
    /// includes a ContentPart of the generic type provided.
    /// </summary>
    /// <typeparam name="TPart">The type of the ContentPart we should have in the content being
    /// tested for the handler to try and replace Permissions.</typeparam>
    public abstract class PermissionOverrideAuthorizationEventHandler<TPart>
        : PermissionOverrideAuthorizationEventHandler
        where TPart : ContentPart {
        /// <summary>
        /// Compute whether the handler should be replacing the Permission against which we are
        /// testing the authorization with the override.
        /// </summary>
        /// <param name="context">The context object for the authorization being tested.</param>
        /// <returns>A boolean that tells the handler whether to go ahead and try to override the
        /// permission.</returns>
        /// <remarks>Specific implementations of this method should generally also use the
        /// result from the base class with base.ShouldOverride(context).</remarks>
        protected override bool ShouldOverride(CheckAccessContext context) {
            return base.ShouldOverride(context)
                && context.Content != null && context.Content.Is<TPart>();
        }
    }
}