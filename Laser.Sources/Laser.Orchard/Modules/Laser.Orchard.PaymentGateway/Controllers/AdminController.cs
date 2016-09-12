using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.PaymentGateway.Controllers {
    public class AdminController : Controller {
        public ActionResult Index() {
            return View("Index");
        }
    }
}