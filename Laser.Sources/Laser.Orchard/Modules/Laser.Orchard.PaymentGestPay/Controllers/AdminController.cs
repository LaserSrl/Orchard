using Laser.Orchard.PaymentGestPay.Services;
using Laser.Orchard.PaymentGestPay.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OMvc = Orchard.Mvc;

namespace Laser.Orchard.PaymentGestPay.Controllers {
    public class AdminController : Controller {

        private IGestPayAdminServices _gestPayAdminServices;

        public AdminController(IGestPayAdminServices gestPayAdminServices) {
            _gestPayAdminServices = gestPayAdminServices;
        }

        public ActionResult Index() {
            //TODO: add permission verification

            return View(_gestPayAdminServices.GetSettingsVM());
        }

        [HttpPost, ActionName("Index")]
        [OMvc.FormValueRequired("submit.SaveSettings")]
        public ActionResult IndexSaveSettings(GestPaySettingsViewModel vm) {
            _gestPayAdminServices.UpdateSettings(vm);
            return RedirectToAction("Index");
        }
    }
}