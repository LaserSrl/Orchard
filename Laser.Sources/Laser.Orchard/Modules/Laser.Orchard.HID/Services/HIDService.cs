using Laser.Orchard.HID.Models;
using Orchard;
using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.HID.Services {
    public class HIDService : IHIDAdminService {

        private readonly IOrchardServices _orchardServices;

        public HIDService(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
        }

        public HIDSiteSettingsPart GetSiteSettings() {
            return _orchardServices.WorkContext.CurrentSite.As<HIDSiteSettingsPart>();
        }
    }
}