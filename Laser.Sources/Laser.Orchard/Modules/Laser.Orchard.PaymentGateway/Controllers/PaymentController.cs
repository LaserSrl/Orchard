using Laser.Orchard.PaymentGateway.Models;
using Laser.Orchard.PaymentGateway.Services;
using Laser.Orchard.PaymentGateway.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Security;
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
                return "";
            }

            public override string GetPosActionUrl(int paymentId) {
                return "";
            }

            public override string GetSettingsControllerName() {
                return "";
            }

            public override string GetPosUrl(int paymentId) {
                return "";
            }
        }
        private readonly PosServiceEmpty _posServiceEmpty;
        public Localizer T { get; set; }

        public PaymentController(IRepository<PaymentRecord> repository, IOrchardServices orchardServices, IEnumerable<IPosService> posServices) {
            _repository = repository;
            _orchardServices = orchardServices;
            _posServices = posServices;
            _posServiceEmpty = new PosServiceEmpty(orchardServices, repository, null);
            T = NullLocalizer.Instance;
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
                    Currency = currency,
                    ContentItemId = itemId
                },
                PosList = _posServices.ToList(),
                ContentItem = item
            };
            model.Record = _posServiceEmpty.StartPayment(model.Record);
            return View("Pay", model);
        }
        [Themed]
        public ActionResult Info(int paymentId) {
            int currentUserId = -1; // utente inesistente
            var user = _orchardServices.WorkContext.CurrentUser;
            if (user != null) {
                currentUserId = user.Id;
            }
            var payment = _repository.Get(paymentId);
            if((_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner) == false)
                && (payment.UserId != currentUserId)) {
                return new HttpUnauthorizedResult();
            }
            return View("Info", payment);
        }
    }
}