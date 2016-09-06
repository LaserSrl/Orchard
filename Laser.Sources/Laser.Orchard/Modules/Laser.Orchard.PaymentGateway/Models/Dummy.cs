using Orchard;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.PaymentGateway.Models {
    public class Dummy : PaymentBase {
        public Dummy(IOrchardServices services, IRepository<PaymentRecord> repository)
            : base(services, repository) {
        }
        public override string PosName {
            get { return "Dummy"; }
        }

        protected override string GetPosUrl(PaymentRecord values) {
            return "/dummy";
        }
    }
}