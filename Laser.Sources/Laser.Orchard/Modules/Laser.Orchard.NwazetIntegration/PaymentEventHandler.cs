using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.PaymentGateway;
using Nwazet.Commerce.Services;
using Nwazet.Commerce.Models;
using Laser.Orchard.NwazetIntegration.Models;

namespace Laser.Orchard.NwazetIntegration {
    public class PaymentEventHandler : IPaymentEventHandler {
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;
        public PaymentEventHandler(IOrderService orderService, IPaymentService paymentService) {
            _orderService = orderService;
            _paymentService = paymentService;
        }
        public void OnError(int paymentId, int contentItemId) {
            var payment = _paymentService.GetPayment(paymentId);
            if (payment != null) {
                var order = _orderService.Get(payment.ContentItemId);
                order.Status = OrderPart.Cancelled;
                order.LogActivity(OrderPart.Error, string.Format("Transaction failed (payment id: {0}).", payment.Id));
            }
        }

        public void OnSuccess(int paymentId, int contentItemId) {
            var payment = _paymentService.GetPayment(paymentId);
            if(payment != null) {
                var order = _orderService.Get(payment.ContentItemId);
                order.Status = OrderPart.Cancelled;
                var charge = new KrakePaymentCharge(payment.PosName) {TransactionId = payment.TransactionId };
                _orderService.CreateOrder(
                    charge, 
                    order.Items, 
                    order.SubTotal, 
                    order.Total, 
                    order.Taxes, 
                    order.ShippingOption, 
                    order.ShippingAddress, 
                    order.BillingAddress, 
                    order.CustomerEmail, 
                    order.CustomerPhone, 
                    order.SpecialInstructions, 
                    OrderPart.Pending, 
                    order.TrackingUrl, 
                    order.IsTestOrder, 
                    order.UserId,
                    (double)(payment.Amount),
                    string.Format("kpo{0}", order.Id),
                    payment.Currency
                );
            }
        }
    }
}