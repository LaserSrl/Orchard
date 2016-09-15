﻿using Laser.Orchard.PaymentGateway.Models;
using Orchard;
using System.Collections.Generic;

public interface IPaymentService : IDependency {
    List<PaymentRecord> GetPayments(int userId, bool lastToFirst = true);
    List<PaymentRecord> GetAllPayments(bool lastToFirst = true);
}