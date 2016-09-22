using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.PaymentGestPay.Models {
    public class GestPayRedirectRecord {
        public virtual int Id { get; set; }
        public virtual int PaymentRecordId { get; set; }
        public virtual string RedirectUrl { get; set; }
        public virtual string Schema { get; set; }

    }
}