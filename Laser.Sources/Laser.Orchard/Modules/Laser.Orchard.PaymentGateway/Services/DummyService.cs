using Laser.Orchard.PaymentGateway.Models;
using Orchard;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.PaymentGateway.Services {
    public class DummyService : PosServiceBase {
        public DummyService(IOrchardServices orchardServices, IRepository<PaymentRecord> repository, IPaymentEventHandler paymentEventHandler)
            : base(orchardServices, repository, paymentEventHandler) {
        }

        public override string GetPosName() {
            return "Dummy";
        }

        public override string GetPosUrl(int paymentId) {
            return "/dummy";
        }
    }
}