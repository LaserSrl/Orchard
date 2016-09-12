using Laser.Orchard.PaymentGestPay.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.PaymentGestPay.Controllers {
    public class TransactionsController : Controller {

        private IGestPayTransactionServices _gestPayTransactionServices;

        public TransactionsController(IGestPayTransactionServices gestPayTransactionServices) {
            _gestPayTransactionServices = gestPayTransactionServices;
        }

        public ActionResult RedirectToGestPayPage(int Id) {
            return Redirect(_gestPayTransactionServices.StartGestPayTransaction(Id));
        }
    }
}