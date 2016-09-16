using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.PaymentCartaSi.Services {
    public interface ICartaSiTransactionService : IDependency {
        string StartCartaSiTransaction(int paymentId);
    }
}