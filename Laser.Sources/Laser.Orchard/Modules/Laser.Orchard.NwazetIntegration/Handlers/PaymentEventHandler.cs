﻿using Laser.Orchard.PaymentGateway;
using Nwazet.Commerce.Services;
using Nwazet.Commerce.Models;
using Laser.Orchard.NwazetIntegration.Services;

namespace Laser.Orchard.NwazetIntegration.Handlers {
    public class PaymentEventHandler : IPaymentEventHandler {
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;
        private readonly IShoppingCart _shoppingCart;
        private readonly IPosServiceIntegration _posServiceIntegration;
        public PaymentEventHandler(IOrderService orderService, IPaymentService paymentService, IShoppingCart shoppingCart, IPosServiceIntegration posServiceIntegration) {
            _orderService = orderService;
            _paymentService = paymentService;
            _shoppingCart = shoppingCart;
            _posServiceIntegration = posServiceIntegration;
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
                // agggiorna l'odine in base al pagamento effettuato
                order.Status = OrderPart.Pending;
                order.AmountPaid = payment.Amount;
                order.PurchaseOrder = _posServiceIntegration.GetOrderNumber(order.Id);
                order.CurrencyCode = payment.Currency;
                order.LogActivity(OrderPart.Event, string.Format("Payed on POS {0}.", payment.PosName));
                // svuota il carrello
                _shoppingCart.Clear();
            }
        }
    }
}