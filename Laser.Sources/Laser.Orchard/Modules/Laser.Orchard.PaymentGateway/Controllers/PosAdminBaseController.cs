using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.PaymentGateway.Controllers {
    public abstract class PosAdminBaseController : Controller {
        protected readonly IOrchardServices _orchardServices;
        public Localizer T { get; set; }

        /// <summary>
        /// Get POS settings part to be used in Index view.
        /// </summary>
        /// <returns></returns>
        protected abstract dynamic GetSettingsPart();

        public PosAdminBaseController(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
            T = NullLocalizer.Instance;
        }
        public ActionResult Index() {
            var settings = GetSettingsPart();
            return View(settings);
        }
        [HttpPost, ActionName("Index")]
        public ActionResult IndexPost() {
            var settings = GetSettingsPart();
            if (TryUpdateModel(settings)) {
                _orchardServices.Notifier.Information(T("Settings saved successfully."));
            }
            else {
                _orchardServices.Notifier.Error(T("Could not save settings."));
            }
            return RedirectToAction("Index");
        }
    }
}