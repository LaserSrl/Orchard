using Laser.Orchard.PaymentGateway.Security;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.PaymentGateway.Controllers {
    public class AdminController : Controller {
        private readonly IOrchardServices _orchardServices;

        public AdminController(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
        }
        public ActionResult Index() {
            if (_orchardServices.Authorizer.Authorize(Permissions.ConfigurePayment) == false) {
                return new HttpUnauthorizedResult();
            }
            return View("Index");
        }
    }
}