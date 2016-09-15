using Laser.Orchard.PaymentGateway.Controllers;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.PaymentCartaSi.Controllers {
    public class AdminController : PosAdminBaseController {

        public AdminController(IOrchardServices orchardServices)
            : base(orchardServices) {

        }

        protected override dynamic GetSettingsPart() {
            throw new NotImplementedException();
        }
    }
}