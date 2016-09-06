using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.PaymentGateway.Models {
    public class PaymentRecord {
        public int Id { get; set; }
        public string Reason { get; set; }
        public string PosName { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public string PosUrl { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public bool Success { get; set; }
        public string Error { get; set; }
        public string TransactionId { get; set; }
        public string ReturnUrl { get; set; }
        public string Info { get; set; }
        public int ContentItemId { get; set; }
    }
}