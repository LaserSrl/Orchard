using System.Web.Mvc;
using Orchard;
using Orchard.Localization;
using Orchard.UI.Notify;
using KrakeDefaultTheme.Settings.Models;
using System;
using Orchard.ContentManagement;

namespace KrakeDefaultTheme.Settings.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller {

        public AdminController(
            IOrchardServices services
            ) {
            Services = services;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        public ActionResult Index() {
            var viewModel = Services.WorkContext.CurrentSite.As<ThemeSettingsPart>();
            return View("~/Themes/KrakeDefaultTheme/Views/Admin/OptionsIndex.cshtml", viewModel);
        }

        [HttpPost]
        public ActionResult Index(ThemeSettingsPart viewModel) {
            throw new NotImplementedException();
            return View("OptionsIndex", null);
        }
    }
}
