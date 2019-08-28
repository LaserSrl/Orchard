﻿using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Roles.Constants;
using Orchard.Roles.Models;
using Orchard.Security;
using Orchard.Security.Permissions;

namespace Orchard.Roles.Services {
    public class RolesBasedAuthorizationService : IAuthorizationService {
        private readonly IRoleService _roleService;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IAuthorizationServiceEventHandler _authorizationServiceEventHandler;

        public RolesBasedAuthorizationService(IRoleService roleService, IWorkContextAccessor workContextAccessor, IAuthorizationServiceEventHandler authorizationServiceEventHandler) {
            _roleService = roleService;
            _workContextAccessor = workContextAccessor;
            _authorizationServiceEventHandler = authorizationServiceEventHandler;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }


        public void CheckAccess(Permission permission, IUser user, IContent content) {
            if (!TryCheckAccess(permission, user, content)) {
                throw new OrchardSecurityException(T("A security exception occurred in the content management system.")) {
                    PermissionName = permission.Name,
                    User = user,
                    Content = content
                };
            }
        }

        public bool TryCheckAccess(Permission permission, IUser user, IContent content) {
            var context = new CheckAccessContext { Permission = permission, User = user, Content = content };
            context.Permissions.Add(permission);
            _authorizationServiceEventHandler.Checking(context);
            // test unicity in context.Permissions
            // if a Permission in context.Permissions is overridden by any of the others,
            // it should be removed fomr the list of permissions to be tested.
            // This can be done by recursively calling Checking on contexts created with the "new"
            // permission, and comparing the resulting lists. This process should go on until the
            // list of permissions stops changing.

            foreach (var p in context.Permissions) {
                context.Granted |= InnerCheck(new CheckAccessContext { Permission = p, User = user, Content = content });
            }

            return context.Granted;
        }

        private bool InnerCheck(CheckAccessContext context) {
            for (var adjustmentLimiter = 0; adjustmentLimiter != 3; ++adjustmentLimiter) {
                if (!context.Granted && context.User != null) {
                    if (!String.IsNullOrEmpty(_workContextAccessor.GetContext().CurrentSite.SuperUser) &&
                           String.Equals(context.User.UserName, _workContextAccessor.GetContext().CurrentSite.SuperUser, StringComparison.Ordinal)) {
                        context.Granted = true;
                    }
                }

                if (!context.Granted) {

                    // determine which set of permissions would satisfy the access check
                    var grantingNames = PermissionNames(context.Permission, Enumerable.Empty<string>()).Distinct().ToArray();

                    // determine what set of roles should be examined by the access check
                    var rolesToExamine = new List<string>();
                    if (context.User == null) {
                        rolesToExamine.Add(SystemRoles.Anonymous);
                    }
                    else if (context.User.Has<IUserRoles>()) {
                        // the current user is not null, so get his roles and add "Authenticated" to it
                        rolesToExamine = context.User.As<IUserRoles>().Roles.ToList();

                        // when it is a simulated anonymous user in the admin
                        if (!rolesToExamine.Contains(AnonymousRole[0])) {
                            rolesToExamine = rolesToExamine.Concat(AuthenticatedRole);
                        }
                    }
                    else {
                        // the user is not null and has no specific role, then it's just "Authenticated"
                        rolesToExamine.Add(SystemRoles.Authenticated);
                    }

                    foreach (var role in rolesToExamine) {
                        foreach (var permissionName in _roleService.GetPermissionsForRoleByName(role)) {
                            string possessedName = permissionName;
                            if (grantingNames.Any(grantingName => String.Equals(possessedName, grantingName, StringComparison.OrdinalIgnoreCase))) {
                                context.Granted = true;
                            }

                            if (context.Granted)
                                break;
                        }

                        if (context.Granted)
                            break;
                    }
                }

                context.Adjusted = false;
                _authorizationServiceEventHandler.Adjust(context);
                if (!context.Adjusted)
                    break;
            }

            _authorizationServiceEventHandler.Complete(context);

            return context.Granted;
        }

        private static IEnumerable<string> PermissionNames(Permission permission, IEnumerable<string> stack) {
            // the given name is tested
            yield return permission.Name;

            // iterate implied permissions to grant, it present
            if (permission.ImpliedBy != null && permission.ImpliedBy.Any()) {
                foreach (var impliedBy in permission.ImpliedBy) {
                    // avoid potential recursion
                    if (stack.Contains(impliedBy.Name))
                        continue;

                    // otherwise accumulate the implied permission names recursively
                    foreach (var impliedName in PermissionNames(impliedBy, stack.Concat(new[] { permission.Name }))) {
                        yield return impliedName;
                    }
                }
            }

            yield return StandardPermissions.SiteOwner.Name;
        }

    }
}
