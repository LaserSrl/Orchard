using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.PaymentGestPay.Services {
    public interface IGestPayTransactionServices : IDependency {
        string StartGestPayTransaction(int paymentId);
    }
}