using Laser.Orchard.StartupConfig.ViewModels;
using Orchard;
using Orchard.Environment.Configuration;
using Orchard.Modules.Services;
using Orchard.Security.Permissions;
using Orchard.Roles.Services;
using System;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using System.Collections.Generic;

namespace Laser.Orchard.StartupConfig.Services {

    public interface IUtilsServices : IDependency {

        string TenantPath { get; }

        string StorageMediaPath { get; }

        string VirtualMediaPath { get; }

        string PublicMediaPath { get; }

        void DisableFeature(string featureId);

        void EnableFeature(string featureId);

        bool FeatureIsEnabled(string featureId);

        /// <summary>
        /// This method may be called whenever we need to update some roles' permissions based on known stereotypes.
        /// For example, whenever we add new permissions and stereotypes to a module, we should update that module's
        /// migration, calling this method and giving the new stereotypes as parameters.
        /// </summary>
        /// <param name="stereotypes">An <type>IEnumerable<PermissionStereotype></type> obtained for example by a call to
        /// <example>new Permissions().GetDefaultStereotypes();</example></param>
        void UpdateStereotypesPermissions(IEnumerable<PermissionStereotype> stereotypes);

        Response GetResponse(ResponseType rsptype, string message = "", dynamic data = null);
    }

    public class UtilsServices : IUtilsServices {

        public Response GetResponse(ResponseType rsptype, string message = "", dynamic data = null) {
            Response rsp = new Response();
            rsp.Message = message;
            switch (rsptype) {
                case ResponseType.Success:
                    rsp.Success = true;
                    if (message != "")
                        rsp.Message = message;
                    else
                        rsp.Message = "Successfully Executed";
                    rsp.ErrorCode = ErrorCode.NoError;
                    rsp.Data = data;
                    rsp.ResolutionAction = ResolutionAction.NoAction;
                    break;

                case ResponseType.InvalidUser:
                    rsp.Success = false;
                    if (message != "")
                        rsp.Message = message;
                    else
                        rsp.Message = "Invalid User";
                    rsp.ErrorCode = ErrorCode.InvalidUser;
                    rsp.Data = data;
                    rsp.ResolutionAction = ResolutionAction.Login;
                    break;

                case ResponseType.InvalidXSRF:
                    rsp.Success = false;
                    if (message != "")
                        rsp.Message = message;
                    else
                        rsp.Message = "Invalid Token/csrfToken";
                    rsp.ErrorCode = ErrorCode.InvalidXSRF;
                    rsp.Data = data;
                    rsp.ResolutionAction = ResolutionAction.Login;
                    break;

                case ResponseType.Validation:
                    rsp.Success = false;
                    if (message != "")
                        rsp.Message = message;
                    else
                        rsp.Message = "Validation error";
                    rsp.ErrorCode = ErrorCode.Validation;
                    rsp.Data = data;
                    rsp.ResolutionAction = ResolutionAction.NoAction;
                    break;

                case ResponseType.UnAuthorized:
                    rsp.Success = false;
                    if (message != "")
                        rsp.Message = message;
                    else
                        rsp.Message = "UnAuthorized Action";
                    rsp.ErrorCode = ErrorCode.UnAuthorized;
                    rsp.Data = data;
                    rsp.ResolutionAction = ResolutionAction.NoAction;
                    break;
            }
            return rsp;
        }

        private readonly IModuleService _moduleService;
        private readonly string _tenantPath;
        private readonly string _storageMediaPath; // C:\orchard\media\default
        private readonly string _virtualMediaPath; // ~/Media/Default/
        private readonly string _publicMediaPath; // /Orchard/Media/Default/
        private readonly IRoleService _roleService;

        public UtilsServices(IModuleService moduleService, ShellSettings settings, IRoleService roleService) {
            _moduleService = moduleService;
            _roleService = roleService;

            var mediaPath = HostingEnvironment.IsHosted
                                ? HostingEnvironment.MapPath("~/Media/") ?? ""
                                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Media");

            _storageMediaPath = Path.Combine(mediaPath, settings.Name);
            _virtualMediaPath = "~/Media/" + settings.Name + "/";

            var appPath = "";
            if (HostingEnvironment.IsHosted) {
                appPath = HostingEnvironment.ApplicationVirtualPath;
            }
            if (!appPath.EndsWith("/"))
                appPath = appPath + '/';
            if (!appPath.StartsWith("/"))
                appPath = '/' + appPath;

            _publicMediaPath = appPath + "Media/" + settings.Name + "/";

            _tenantPath = HostingEnvironment.IsHosted ? HostingEnvironment.MapPath("~/") ?? "" : AppDomain.CurrentDomain.BaseDirectory;
        }

        /// <summary>
        /// Returns the tenant path
        /// </summary>
        public string TenantPath { get { return _tenantPath; } }

        /// <summary>
        /// Returns the media path in the format C:\orchard\media\default\
        /// </summary>
        public string StorageMediaPath { get { return _storageMediaPath; } }

        /// <summary>
        /// Returns the media path in the format ~/Media/Default/
        /// </summary>
        public string VirtualMediaPath { get { return _virtualMediaPath; } }

        /// <summary>
        /// Returns the media path in the format /Orchard/Media/Default/
        /// </summary>
        public string PublicMediaPath { get { return _publicMediaPath; } }

        public void DisableFeature(string featureId) {
            var features = _moduleService.GetAvailableFeatures().ToDictionary(m => m.Descriptor.Id, m => m);
            if (features.ContainsKey(featureId) && features[featureId].IsEnabled) {
                _moduleService.DisableFeatures(new string[] { featureId });
            }
        }

        public void EnableFeature(string featureId) {
            var features = _moduleService.GetAvailableFeatures().ToDictionary(m => m.Descriptor.Id, m => m);

            if (features.ContainsKey(featureId) && !features[featureId].IsEnabled) {
                _moduleService.EnableFeatures(new string[] { featureId },true);
            }
        }

        public bool FeatureIsEnabled(string featureId) {
            var features = _moduleService.GetAvailableFeatures().ToDictionary(m => m.Descriptor.Id, m => m);
            return (features.ContainsKey(featureId) && features[featureId].IsEnabled);
        }

        public void UpdateStereotypesPermissions(IEnumerable<PermissionStereotype> stereotypes) {
            foreach (var stereotype in stereotypes) {
                //get role corresponding to the stereotype
                var role = _roleService.GetRoleByName(stereotype.Name);
                if (role == null) {
                    //create new role
                    _roleService.CreateRole(stereotype.Name);
                    role = _roleService.GetRoleByName(stereotype.Name);
                }
                //merge permissions into the role
                var stereotypePermissionsNames = (stereotype.Permissions ?? Enumerable.Empty<Permission>()).Select(x => x.Name);
                var currentPermissionsNames = role.RolesPermissions.Select(x => x.Permission.Name);
                var distinctPerrmissionsNames = currentPermissionsNames
                    .Union(stereotypePermissionsNames)
                    .Distinct();
                //if we added permissions we update the role
                var additionalPermissionsNames = distinctPerrmissionsNames.Except(currentPermissionsNames);
                if (additionalPermissionsNames.Any()) {
                    //we have new permissions to add to this role
                    foreach (var permissionName in additionalPermissionsNames) {
                        _roleService.CreatePermissionForRole(role.Name, permissionName);
                    }
                }
            }
        }
    }
}