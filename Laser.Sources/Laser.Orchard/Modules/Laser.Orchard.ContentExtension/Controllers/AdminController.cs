using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
//using Laser.Orchard.CulturePicker.Services;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Notify;
using Orchard;
using Laser.Orchard.ContentExtension.Models;
using Laser.Orchard.ContentExtension.Services;


namespace Laser.Orchard.ContentExtension.Controllers {
    public class AdminController : Controller {
        private readonly IContentTypePermissionSettingsService _contentTypePermissionSettingsService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IMembershipService _membershipService;
        private readonly IOrchardServices _orchardServices;

        public Localizer T { get; set; }
        // GET: /Admin/
        public AdminController(IContentTypePermissionSettingsService contentTypePermissionSettingsService,
            IAuthenticationService authenticationService,
            IMembershipService membershipService, IOrchardServices orcharcServices) {
            _contentTypePermissionSettingsService = contentTypePermissionSettingsService;
            _authenticationService = authenticationService;
            _membershipService = membershipService;
            _orchardServices = orcharcServices;
            T = NullLocalizer.Instance;
        }

        [HttpGet]
        public ActionResult Settings() {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Yout have to be an Administrator to edit ContentTypePermission settings!")))
                return new HttpUnauthorizedResult();
            var model = _contentTypePermissionSettingsService.ReadSettings();
            return View(model);
        }
        [HttpPost]
        public ActionResult Settings(SettingsModel model) {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Yout have to be an Administrator to edit ContentTypePermission settings!")))
                return new HttpUnauthorizedResult();
            if (!ModelState.IsValid) {
                _orchardServices.Notifier.Error(T("Settings update failed: {0}", T("check your input!")));
                return View(model);
            }
            try {
                _contentTypePermissionSettingsService.WriteSettings(model);
                _orchardServices.Notifier.Information(T("ContentType Permission settings updated."));
                // I read again my model in order to its ids
                model = _contentTypePermissionSettingsService.ReadSettings();
            }
            catch (Exception exception) {
                _orchardServices.Notifier.Error(T("Settings update failed: {0}", exception.Message));
            }
            return RedirectToAction("Settings");
        }
    }
}
