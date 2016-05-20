using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Autoroute.Models;

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

        public IContent GetContentByAlias(string displayAlias) {
            IContent item = null;
            var autoroutePart = _orchardServices.ContentManager.Query<AutoroutePart, AutoroutePartRecord>()
                .ForVersion(VersionOptions.Published)
                .Where(w => w.DisplayAlias == displayAlias).List().SingleOrDefault();

            if (autoroutePart != null && autoroutePart.ContentItem != null) {
                item = autoroutePart.ContentItem;
            } else {
                new HttpException(404, ("Not found"));
                return null;
            }
            return item;

        }
    }
}