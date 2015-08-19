using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Laser.Orchard.Payment.Services;
using Orchard;
using Orchard.UI.Notify;
using Orchard.Localization;
using Laser.Orchard.Payment.Models;
using Orchard.DisplayManagement;

namespace Laser.Orchard.Payment.Controllers {
    public class PaymentController : Controller {
        //////////////////////////////////
        //
        // THIS CONTROLLER IS NEVER USER ... CAN BE DELETED.... 
        // TO USE ONLY FOR TEST PAYMENT
        //
        /////////////////////////////////
        // fase 1 Payment

        private readonly IPaymentService _paymentService;
        private readonly INotifier _notifier;
        public Localizer T { get; set; }
        private dynamic Shape { get; set; }
        

        public PaymentController(  IShapeFactory shapeFactory,
            IPaymentService paymentService,
            INotifier notifier
            ) {
            _paymentService = paymentService;
            _notifier = notifier;
            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

        public ActionResult ResultOK(string a ,string b) {
            Shape.Esito= "OK";
            return View(Shape);
        }
        public ActionResult ResultKO(string a, string b) {
            Shape.Esito= "KO";
            return View(Shape);
        }
        public ActionResult ResultS2S(string a, string b) {

            TransazioneRicevuta esito = _paymentService.GestPayRiceviTranzazioneS2S(a, b);
            if (esito == null) {
                _notifier.Add(NotifyType.Information, T("transazione non effettuata"));
            } else
                _notifier.Add(NotifyType.Information, T("transazione effettuata"));
            Shape.Esito= "S2S";
            return View(Shape); 
        }

        public ActionResult Test() {
            // simulazione di pagamento
            Transazione trans = new Transazione();
            trans.Amount = 0.01;
          //  if (1 == 2) {
               // trans.BuyerName = "nome del customerxyz";
           //    trans.BuyerEmail = "email@ditest.it";
               var info = new Dictionary<string, string>();
              info.Add("order_number", "18");
                info.Add("Customer", "nomedelcustomerxyz"); //To GENERATE ERROR
                info.Add("Customeremail", "email@ditest.it");
               trans.CustomInfoGenerate = info;
               // trans.Language = ListaCodiciLingua.Italiano;
        //    }
            string url_ToLoad = "";
            try {
                url_ToLoad = _paymentService.GestPayAvviaTransazione(trans);
            } catch (Exception ex) {
                _notifier.Add(NotifyType.Information, T("Service unavailable, retry"));
                return null;
            }
            if (url_ToLoad.Length > 0)
                return Redirect(url_ToLoad);
            else
                return null;
        }
    }
}