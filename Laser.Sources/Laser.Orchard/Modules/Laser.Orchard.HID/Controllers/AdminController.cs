using Laser.Orchard.HID.Services;
using Orchard;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.HID.Controllers {
    [Admin]
    public class AdminController : Controller {

        private readonly IOrchardServices _orchardServices;
        private readonly IHIDAdminService _HIDAdminService;

        public Localizer T { get; set; }

        public AdminController(IOrchardServices orchardServices, IHIDAdminService HIDAdminService) {
            _orchardServices = orchardServices;
            _HIDAdminService = HIDAdminService;

            T = NullLocalizer.Instance;
        }

        public ActionResult Index() {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not allowed to manage HID settings")))
                return new HttpUnauthorizedResult();
            return View(_HIDAdminService.GetSiteSettings());
        }
        [HttpPost, ActionName("Index")]
        public ActionResult IndexPost() {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not allowed to manage HID settings")))
                return new HttpUnauthorizedResult();
            var settings = _HIDAdminService.GetSiteSettings();
            if (TryUpdateModel(settings)) {
                _orchardServices.Notifier.Information(T("Settings saved successfully."));
            } else {
                _orchardServices.Notifier.Error(T("Could not save settings."));
            }
            return RedirectToAction("Index");
        }
    }
}