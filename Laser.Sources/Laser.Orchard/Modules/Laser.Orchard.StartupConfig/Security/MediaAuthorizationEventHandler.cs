using Laser.Orchard.StartupConfig.Services;
using Orchard;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;

namespace Laser.Orchard.StartupConfig.Security {
    public class MediaAuthorizationEventHandler : IAuthorizationServiceEventHandler {
        private readonly IOrchardServices _orchardServices;
        private readonly IControllerContextAccessor _controllerContextAccessor;
        public Localizer T { get; set; }
        public ILogger Log { get; set; }
        public MediaAuthorizationEventHandler(IControllerContextAccessor controllerContextAccessor, IOrchardServices orchardServices) {
            _controllerContextAccessor = controllerContextAccessor;
            _orchardServices = orchardServices;
            T = NullLocalizer.Instance;
            Log = NullLogger.Instance;
        }

        ControllerContext controllerContext { get; set; }
        public void Checking(CheckAccessContext context) { }
        public void Complete(CheckAccessContext context) { }
        public void Adjust(CheckAccessContext context) {
            if (_controllerContextAccessor != null && _controllerContextAccessor.Context != null) {
                if (_controllerContextAccessor.Context.Controller.GetType().FullName == "Orchard.MediaLibrary.Controllers.AdminController") {
                    if (_controllerContextAccessor.Context.DisplayMode == null) {
                        if (context.Permission.Name.Equals("ManageMediaContent")) {
                            context.Adjusted = true;
                            context.Permission = MediaPermissions.ViewMedia;
                        }
                    }
                }
              //  if (_controllerContextAccessor.Context.Controller.GetType().FullName == "Orchard.Core.Contents.Controllers.AdminController") {
                if (context.Content != null && context.Content.ContentItem != null && (context.Content.ContentItem.ContentType == "Image" || context.Content.ContentItem.ContentType == "OEmbed" || context.Content.ContentItem.ContentType == "Audio" || context.Content.ContentItem.ContentType == "Document" || context.Content.ContentItem.ContentType == "Video")) {
             //       if (_controllerContextAccessor.Context.DisplayMode == null) {
                        if (context.Permission.Name.Equals("EditContent")) {
                            context.Adjusted = true;
                            context.Permission = MediaPermissions.EditMedia;
               //         }
                    }
                }
              //  }
            }
        }
    }
}