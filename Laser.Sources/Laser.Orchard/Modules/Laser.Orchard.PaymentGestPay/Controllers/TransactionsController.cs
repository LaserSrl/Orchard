using Laser.Orchard.PaymentGestPay.Models;
using Laser.Orchard.PaymentGestPay.Services;
using Orchard.DisplayManagement;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.PaymentGestPay.Controllers {
    public class TransactionsController : Controller {

        private IGestPayTransactionServices _gestPayTransactionServices;
        private dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public TransactionsController(IShapeFactory shapeFactory, IGestPayTransactionServices gestPayTransactionServices) {
            _gestPayTransactionServices = gestPayTransactionServices;
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public ActionResult RedirectToGestPayPage(int Id) {
            return Redirect(_gestPayTransactionServices.StartGestPayTransactionURL(Id));
        }

        //GestPay calls this controller while proceeding with the transaction
        public ActionResult GestPayS2SEndpoint(string a, string b) {
            TransactionOutcome outcome = _gestPayTransactionServices.ReceiveS2STransaction(a, b);

            string shapeString = "S2S";

            if (!string.IsNullOrWhiteSpace(outcome.TransactionResult)) {
                if (outcome.TransactionResult == "KO") {
                    shapeString += T(" Transaction failed with error: {0}", outcome.ErrorDescription).Text;
                } else if (outcome.TransactionResult == "OK") {
                    shapeString += T("Transaction successful.").Text;
                } else {
                    shapeString += T(" Transactions still in progress.").Text;
                }
            }

            Shape.Outcome = shapeString;
            return View(Shape); ;
        }

        //GestPay redirects the buyer to these actions when it's done
        public ActionResult GestPaySuccess(string a, string b) {
            return RedirectToAction("GestPayOutcome", new { a = a, b = b });
        }
        public ActionResult GestPayFailure(string a, string b) {
            return RedirectToAction("GestPayOutcome", new { a = a, b = b });
        }
        public ActionResult GestPayOutcome(string a, string b) {
            return Redirect(_gestPayTransactionServices.InterpretTransactionResult(a, b));
        }
    }
}