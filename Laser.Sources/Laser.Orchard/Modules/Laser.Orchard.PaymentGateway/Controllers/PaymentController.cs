using Laser.Orchard.PaymentGateway.Models;
using Laser.Orchard.PaymentGateway.Services;
using Laser.Orchard.PaymentGateway.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Themes;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Laser.Orchard.PaymentGateway.Controllers {
    public class PaymentController : Controller {
        private readonly IRepository<PaymentRecord> _repository;
        private readonly IOrchardServices _orchardServices;
        private readonly IEnumerable<IPosService> _posServices;
        private class PosServiceEmpty : PosServiceBase {
            public PosServiceEmpty(IOrchardServices orchardServices, IRepository<PaymentRecord> repository, IPaymentEventHandler paymentEventHandler)
                : base(orchardServices, repository, paymentEventHandler) {
            }
            public override string GetPosName() {
                return "Empty";
            }

            public override string GetPosUrl(PaymentRecord values) {
                return "/";
            }
        }
        private readonly PosServiceEmpty _posServiceEmpty;

        public PaymentController(IRepository<PaymentRecord> repository, IOrchardServices orchardServices, IEnumerable<IPosService> posServices) {
            _repository = repository;
            _orchardServices = orchardServices;
            _posServices = posServices;
            _posServiceEmpty = new PosServiceEmpty(orchardServices, repository, null);
        }
        [Themed]
        public ActionResult Pay(string reason, decimal amount, string currency, int itemId = 0) {
            ContentItem item = null;
            if (itemId > 0) {
                item = _orchardServices.ContentManager.Get(itemId);
            }
            PaymentVM model = new PaymentVM {
                Record = new PaymentRecord {
                    Reason = reason,
                    Amount = amount,
                    Currency = currency
                },
                PosList = _posServices.ToList(),
                ContentItem = item
            };
            model.Record = _posServiceEmpty.StartPayment(model.Record);
            return View("Pay", model);
        }
        [Themed]
        public ActionResult Info(int paymentId) {
            var payment = _repository.Get(paymentId);
            return View("Info", payment);
        }
    }
}