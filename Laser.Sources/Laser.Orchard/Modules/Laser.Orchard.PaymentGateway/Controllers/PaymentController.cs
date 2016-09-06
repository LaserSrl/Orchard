using Laser.Orchard.PaymentGateway.Models;
using Laser.Orchard.PaymentGateway.ViewModels;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.PaymentGateway.Controllers {
    public class PaymentController : Controller {
        private readonly IEnumerable<IPayment> _posList;
        private readonly IRepository<PaymentRecord> _repository;

        public PaymentController(IEnumerable<PaymentBase> posList, IRepository<PaymentRecord> repository) {
            _posList = posList;
            _repository = repository;
        }
        public ActionResult Pay(string reason, decimal amount, string currency) {
            List<string> posNames = new List<string>();
            posNames.Add("Dummy"); // _posList.Select(x => x.PosName).ToList();
            PaymentVM model = new PaymentVM {
                Reason = reason,
                Amount = amount,
                Currency = currency,
                PosList = posNames
            };
            return View("Pay", model);
        }
        public ActionResult Info(int paymentId) {
            var payment = _repository.Get(paymentId);
            return View("Info", payment);
        }
    }
}