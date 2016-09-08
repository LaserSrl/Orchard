using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Laser.Orchard.PaymentGestPay.Models {
    public class GestPayTransaction {

        [StringLength(3)]
        public string uicCode { get; set; } //MANDATORY currency code
        [StringLength(9)]
        public string amount { get; set; } //MANDATORY
        [StringLength(50)]
        public string shopTransactionID { get; set; } //MANDATORY

    }
}