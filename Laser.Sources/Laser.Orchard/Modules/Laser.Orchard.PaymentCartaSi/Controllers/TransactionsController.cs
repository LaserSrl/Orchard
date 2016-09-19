﻿using Laser.Orchard.PaymentCartaSi.Services;
using Orchard.DisplayManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.PaymentCartaSi.Controllers {
    public class TransactionsController : Controller {

        private ICartaSiTransactionService _cartaSiTransactionService;
        private dynamic Shape { get; set; }

        public TransactionsController(ICartaSiTransactionService tService, IShapeFactory shapeFactory) {
            _cartaSiTransactionService = tService;
            Shape = shapeFactory;
        }

        public ActionResult RedirectToCartaSìPage(int Id) {
            return Redirect(_cartaSiTransactionService.StartCartaSiTransaction(Id));
        }

        [HttpPost]
        public ActionResult CartaSiS2S() {
            //read the querystring that contains the transaction results (Request.QueryString)
            //this is called in POST by the CartaSì server.
            //we need to do it because it may ont be really hard to cheat if we only used stuff on the client's side

            string shapeString = "S2S";

            shapeString += _cartaSiTransactionService.HandleS2STransaction(Request.Form);
            Shape.Outcome = shapeString;
            return View(Shape);
        }

        public ActionResult CartaSiOutcome() {
            //read the querystring that contains the transaction results (Request.QueryString)
            return Redirect(_cartaSiTransactionService.HandleOutcomeTransaction(Request.QueryString));
        }

        public ActionResult CartaSiUndo(string importo, string divisa, string codTrans, string esito) {
            //this gets called when the transaction was canceled or if a call had errors
            return Redirect(_cartaSiTransactionService.ReceiveUndo(importo, divisa, codTrans, esito));
        }
    }
}