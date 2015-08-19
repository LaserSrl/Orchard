using Laser.Orchard.StartupConfig.ViewModels;
using Orchard;
using Orchard.Environment.Configuration;
using Orchard.Modules.Services;
using System;
using System.IO;
using System.Linq;
using System.Web.Hosting;

namespace Laser.Orchard.StartupConfig.Services {

    public interface IUtilsServices : IDependency {

        string StorageMediaPath { get; }

        string VirtualMediaPath { get; }

        string PublicMediaPath { get; }

        void DisableFeature(string featureId);

        void EnableFeature(string featureId);

        bool FeatureIsEnabled(string featureId);

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
        private readonly string _storageMediaPath; // C:\orchard\media\default
        private readonly string _virtualMediaPath; // ~/Media/Default/
        private readonly string _publicMediaPath; // /Orchard/Media/Default/

        public UtilsServices(IModuleService moduleService, ShellSettings settings) {
            _moduleService = moduleService;

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
        }

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
                _moduleService.EnableFeatures(new string[] { featureId });
            }
        }

        public bool FeatureIsEnabled(string featureId) {
            var features = _moduleService.GetAvailableFeatures().ToDictionary(m => m.Descriptor.Id, m => m);
            return (features.ContainsKey(featureId) && features[featureId].IsEnabled);
        }
    }
}