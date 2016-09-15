using Laser.Orchard.PaymentCartaSi.Extensions;
using Laser.Orchard.PaymentGateway;
using Laser.Orchard.PaymentGateway.Models;
using Laser.Orchard.PaymentGateway.Services;
using Orchard;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.PaymentCartaSi.Services {
    public class CartaSiPosService : PosServiceBase {

        public CartaSiPosService(IOrchardServices orchardServices, IRepository<PaymentRecord> repository, IPaymentEventHandler paymentEventHandler) :
            base(orchardServices, repository, paymentEventHandler) {

        }

        public override string GetPosName() {
            return Constants.PosName;
        }
        public override string GetSettingsControllerName() {
            return "Admin";
        }
        public override string GetPosUrl(int paymentId) {
            throw new NotImplementedException();
        }


    }
}