using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.PaymentGateway.ViewModels {
    public class PaymentVM {
        public string Reason { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Info { get; set; }
        public int ContentItemId { get; set; }
        public List<string> PosList { get; set; }
    }
}