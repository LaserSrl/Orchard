using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Laser.Orchard.NwazetIntegration.Services;
using Laser.Orchard.NwazetIntegration.ViewModels;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Orchard.Themes;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Laser.Orchard.NwazetIntegration.Models;

namespace Laser.Orchard.NwazetIntegration.Controllers {
    public class AddressesController : Controller {
        private readonly IOrderService _orderService;
        private readonly IPosServiceIntegration _posServiceIntegration;
        private readonly IShoppingCart _shoppingCart;
        public AddressesController(IOrderService orderService, IPosServiceIntegration posServiceIntegration, IShoppingCart shoppingCart) {
            _orderService = orderService;
            _posServiceIntegration = posServiceIntegration;
            _shoppingCart = shoppingCart;
        }
        
        public ActionResult Index(AddressesVM model) {
            ActionResult result = null;
            switch (model.Submit) {
                case "cart":
                    result = RedirectToAction("Index", "ShoppingCart", new { area = "Nwazet.Commerce" });
                    break;
                case "save":
                    // costruisce la lista di CheckoutItems in base al contenuto del carrello
                    List<CheckoutItem> items = new List<CheckoutItem>();
                    foreach (var prod in _shoppingCart.GetProducts()) {
                        items.Add(new CheckoutItem {
                            Attributes = prod.AttributeIdsToValues,
                            LinePriceAdjustment = prod.LinePriceAdjustment,
                            OriginalPrice = prod.OriginalPrice,
                            Price = prod.Price,
                            ProductId = prod.Product.Id,
                            PromotionId = prod.Promotion == null ? null : (int?)(prod.Promotion.Id),
                            Quantity = prod.Quantity,
                            Title = prod.Product.ContentItem.As<TitlePart>().Title
                        });
                    }
                    var paymentGuid = Guid.NewGuid().ToString();
                    var charge = new KrakePaymentCharge("Payment Gateway", paymentGuid);
                    var currency = "USD"; // TODO: leggere la currency dai settings
                    var order = _orderService.CreateOrder(
                        charge, 
                        items, 
                        _shoppingCart.Subtotal(), 
                        _shoppingCart.Total(), 
                        _shoppingCart.Taxes(), 
                        _shoppingCart.ShippingOption, 
                        model.ShippingAddress, 
                        model.BillingAddress, 
                        model.Email, 
                        model.Phone, 
                        model.SpecialInstructions, 
                        OrderPart.Cancelled, 
                        null, 
                        false, 
                        -1, 
                        0, 
                        "", 
                        currency);
                    var reason = string.Format("Purchase Order kpo{0}", order.Id);
                    result = RedirectToAction("Pay", "Payment", new { area = "Laser.Orchard.PaymentGateway", reason = reason, amount = order.Total, currency = order.CurrencyCode, itemId = order.Id, newPaymentGuid = paymentGuid });
                    break;
                default:
                    model.ShippingAddress = new Address();
                    model.BillingAddress = new Address();
                    result = View("Index", model);
                    break;
            }
            return result;
        }
    }
}