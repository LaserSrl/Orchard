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
        private readonly IHIDAPIService _HIDAPISerivce;

        public Localizer T { get; set; }

        public AdminController(IOrchardServices orchardServices,
            IHIDAdminService HIDAdminService,
            IHIDAPIService hIDAPISerivce) {
            _orchardServices = orchardServices;
            _HIDAdminService = HIDAdminService;
            _HIDAPISerivce = hIDAPISerivce;

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
                //attempt authentication
                switch (_HIDAPISerivce.Authenticate()) {
                    case AuthenticationErrors.NoError:
                        _orchardServices.Notifier.Information(T("Authentication OK."));
                        break;
                    case AuthenticationErrors.NotAuthenticated:
                        _orchardServices.Notifier.Error(T("Unable to attempt authentication."));
                        break;
                    case AuthenticationErrors.ClientInfoInvalid:
                        _orchardServices.Notifier.Error(T("Client information invalid: Authentication failed."));
                        break;
                    case AuthenticationErrors.CommunicationError:
                        _orchardServices.Notifier.Error(T("Communication errors: Authentication failed."));
                        break;
                    default:
                        break;
                }
                var u = _HIDAPISerivce.SearchHIDUser(_orchardServices.WorkContext.CurrentUser);
                var inv = u.User.CreateInvitation();
            } else {
                _orchardServices.Notifier.Error(T("Could not save settings."));
            }
            return RedirectToAction("Index");
        }
    }
}