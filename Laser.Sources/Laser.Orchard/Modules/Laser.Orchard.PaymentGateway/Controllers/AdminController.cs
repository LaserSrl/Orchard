using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.PaymentGateway.Controllers {
    /// <summary>
    /// I controller che ereditano da questa classe devono chiamarsi "AdminController" per mantenere la grafica di back-end.
    /// </summary>
    public class AdminController : Controller {
        public ActionResult Index() {
            return View("Index");
        }
    }
}