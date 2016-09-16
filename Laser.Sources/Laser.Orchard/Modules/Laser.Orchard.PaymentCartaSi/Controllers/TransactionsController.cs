using Laser.Orchard.PaymentCartaSi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.PaymentCartaSi.Controllers {
    public class TransactionsController : Controller {

        private ICartaSiTransactionService _cartaSiTransactionService;

        public TransactionsController(ICartaSiTransactionService tService) {
            _cartaSiTransactionService = tService;
        }

        public ActionResult RedirectToCartaSìPage(int Id) {
            return Redirect(_cartaSiTransactionService.StartCartaSiTransaction(Id));
        }

        public ActionResult CartaSiOutcome() {
            //read the querystring that contains the transaction results (Request.QueryString)
            return null;
        }

        public ActionResult CartaSiUndo(string importo, string divisa, string codTrans, string esito) {
            //this gets called when the transaction was canceled or if a call had errors
            return Redirect(_cartaSiTransactionService.ReceiveUndo(importo, divisa, codTrans, esito));
        }
    }
}