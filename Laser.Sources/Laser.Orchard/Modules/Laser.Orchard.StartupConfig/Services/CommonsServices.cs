using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard;

namespace Laser.Orchard.StartupConfig.Services {
    public class CommonsServices : ICommonsServices {
        private readonly IOrchardServices _orchardServices;
        public CommonsServices(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
        }

        public DevicesBrands GetDeviceBrandByUserAgent() {
            var userAgent = _orchardServices.WorkContext.HttpContext.Request.UserAgent.ToLower().Trim();
            if (userAgent.Contains("iphone") || userAgent.Contains("ipod") || userAgent.Contains("ipad")) {
                return DevicesBrands.Apple;
            } else if (userAgent.Contains("windows")) {
                return  DevicesBrands.Windows;
            } else if (userAgent.Contains("android")) {
                return DevicesBrands.Google;
            } else {
                return DevicesBrands.Unknown;
            }

        }
    }
}