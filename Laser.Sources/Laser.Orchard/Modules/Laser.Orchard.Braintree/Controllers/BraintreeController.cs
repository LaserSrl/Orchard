using Laser.Orchard.Braintree.Models;
using Laser.Orchard.Braintree.Services;
using Laser.Orchard.Braintree.ViewModels;
using Laser.Orchard.PaymentGateway;
using Laser.Orchard.PaymentGateway.Models;
using Newtonsoft;
using Newtonsoft.Json;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Themes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.Braintree.Controllers {
    public class BraintreeController : Controller {
        private readonly IOrchardServices _orchardServices;
        private readonly BraintreePosService _posService;
        private readonly IBraintreeService _braintreeService;

        public BraintreeController(IOrchardServices orchardServices, IRepository<PaymentRecord> repository, IPaymentEventHandler paymentEventHandler, IBraintreeService braintreeService)
        {
            _orchardServices = orchardServices;
            _posService = new BraintreePosService(orchardServices, repository, paymentEventHandler);
            _braintreeService = braintreeService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pid">Payment ID</param>
        /// <returns></returns>
        [Themed]
        public ActionResult Index(int pid)
        {
            PaymentVM model = new PaymentVM();
            PaymentRecord payment = _posService.GetPaymentInfo(pid);
            model.Record = payment;
            model.TenantBaseUrl = Url.Action("Index").Replace("/Laser.Orchard.Braintree/Braintree", "");
            return View("Index", model);
        }

        [HttpGet]
        public ActionResult GetToken() {
            var clientToken = _braintreeService.GetClientToken();
            return Content(clientToken, "text/plain", Encoding.UTF8);
        }

        [Themed]
        [HttpPost]
        public ActionResult Pay() {
            string nonce = Request["payment_method_nonce"];
            string sAmount = Request["amount"];
            string sPid = Request["pid"];
            decimal amount = decimal.Parse(sAmount, CultureInfo.InvariantCulture);
            int pid = int.Parse(sPid);
            var payResult = _braintreeService.Pay(nonce, amount, null);
            string error = "";
            string transactionId = "";
            if (payResult.Success == false) {
                error = payResult.ResponseText;
            }
            else {
                // pagamento ok
                transactionId = payResult.TransactionId;
            }
            string info = JsonConvert.SerializeObject(payResult);
            _posService.EndPayment(pid, payResult.Success, error, info, transactionId);
            return Redirect(_posService.GetPaymentInfoUrl(pid));
        }
    }
}