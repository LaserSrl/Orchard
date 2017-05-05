using System.Collections.Generic;
using System.Linq;
using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.PaymentGateway.Models;
using Laser.Orchard.PaymentGateway.ViewModels;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Orchard;
using Orchard.DisplayManagement;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using System.Web.Mvc;

namespace Laser.Orchard.NwazetIntegration.Services {
    public interface IPosServiceIntegration : ICheckoutService {
    }

    public class PosServiceIntegration : IPosServiceIntegration {
        private readonly IOrchardServices _orchardServices; 
        private readonly IEnumerable<IPosService> _posServices;
        private readonly dynamic _shapeFactory;
        private readonly IOrderService _orderService;
        private readonly ICurrencyProvider _currencyProvider;
        private readonly IPaymentService _paymentService;

        public PosServiceIntegration(
            IOrchardServices orchardServices, 
            IEnumerable<IPosService> posServices, 
            IShapeFactory shapeFactory,
            IOrderService orderService,
            ICurrencyProvider currencyProvider,
            IPaymentService paymentService) {
            _orchardServices = orchardServices;
            _posServices = posServices;
            _shapeFactory = shapeFactory;
            _orderService = orderService;
            _currencyProvider = currencyProvider;
            _paymentService = paymentService;
        }

        public string Name
        {
            get
            {
                return "Krake payments";
            }
        }

        public dynamic BuildCheckoutButtonShape(IEnumerable<dynamic> productShapes, IEnumerable<ShoppingCartQuantityProduct> productQuantities, IEnumerable<ShippingOption> shippingOptions, TaxAmount taxes, string country, string zipCode, IEnumerable<string> custom) {
            var charge = new KrakePaymentCharge();
            List<CheckoutItem> items = new List<CheckoutItem>();
            double subTotal = 0;
            double total = 0;
            foreach(var prod in productQuantities) {
                items.Add(new CheckoutItem {
                    Attributes =prod.AttributeIdsToValues,
                    LinePriceAdjustment =prod.LinePriceAdjustment,
                    OriginalPrice = prod.OriginalPrice,
                    Price = prod.Price,
                    ProductId = prod.Product.Id,
                    PromotionId = prod.Promotion == null ? null : (int?)(prod.Promotion.Id),
                    Quantity = prod.Quantity,
                    Title = prod.Product.ContentItem.As<TitlePart>().Title
                });
                subTotal += prod.Price;
            }
            total = subTotal;
            bool insertOrder = false;
            foreach(var opt in shippingOptions) {
                if(opt != null) {
                    insertOrder = true;
                    total += opt.Price;
                }
            }
            if (insertOrder) {
                var currency = "USD"; // TODO: leggere la currency dai settings
                var order = _orderService.CreateOrder(charge, items, subTotal, total, taxes, null, null, null, null, null, null, OrderPart.Cancelled, null, false, -1, 0, "", currency);
                return _shapeFactory.Pos(OrderId: order.Id);
            }
            else {
                return null;
            }
        }
        public string GetChargeAdminUrl(string transactionId) {
            string result = "";
            var payment = _paymentService.GetPaymentByTransactionId(transactionId);
            if(payment != null) {
                var urlHelper = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
                var url = urlHelper.Action("Info", "Payment", new { area = "Laser.Orchard.PaymentGateway" });
                result = string.Format("{0}?paymentId={1}", url, payment.Id);
            }
            return result;
        }
        private PaymentVM GetPaymentInfo(OrderPart order) {
            decimal amount = new decimal(order.Total);
            PaymentVM payment = new PaymentVM {
                Record = new PaymentRecord {
                    Reason = string.Format("Purchase Order kpo{0}", order.Id),
                    Amount = amount,
                    Currency = order.CurrencyCode,
                    ContentItemId = order.Id
                },
                PosList = _posServices.ToList(),
                ContentItem = null
            };
            return payment;
        }
    }
}