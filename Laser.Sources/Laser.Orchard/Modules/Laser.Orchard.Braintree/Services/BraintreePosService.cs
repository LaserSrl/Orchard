using Laser.Orchard.PaymentGateway.Services;
using Laser.Orchard.PaymentGateway.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Orchard;
using Orchard.Data;
using Laser.Orchard.PaymentGateway;
using System.Text;
using System.Globalization;

namespace Laser.Orchard.Braintree.Services {
    public class BraintreePosService : PosServiceBase {
        public BraintreePosService(IOrchardServices orchardServices, IRepository<PaymentRecord> repository, IPaymentEventHandler paymentEventHandler)
            : base(orchardServices, repository, paymentEventHandler) {
        }
        public override string GetPosName() {
            return "Braintree and PayPal";
        }
        public override string GetPosUrl(int paymentId) {
            UrlHelper urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
            return urlHelper.Action("Index", "Braintree", new { area = "Laser.Orchard.Braintree" })
                + "?pid=" + paymentId.ToString();
        }

        public override string GetSettingsControllerName() {
            return "Admin";
        }
    }
}