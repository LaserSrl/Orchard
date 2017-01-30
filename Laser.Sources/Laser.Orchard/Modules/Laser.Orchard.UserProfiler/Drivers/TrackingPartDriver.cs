using Laser.Orchard.UserProfiler.Models;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Web;
using System.Web.Http.Controllers;

namespace Laser.Orchard.UserProfiler.Drivers {
    public class TrackingPartDriver : ContentPartDriver<TrackingPart> {
        private readonly ShellSettings _shellsetting;
        public TrackingPartDriver(ShellSettings shellsetting) {
             _shellsetting=shellsetting;
        }
        protected override string Prefix {
            get { return "TrackingPart"; }
        }

        protected override DriverResult Display(TrackingPart part, string displayType, dynamic shapeHelper) {

         //   var host = _shellsetting.RequestUrlHost;
         //   var prefix = _shellsetting.RequestUrlPrefix;
        
            return ContentShape("Parts_Tracking_Display", () => shapeHelper.Parts_Tracking_Display(PageID: part.Id));
        }
    }
}