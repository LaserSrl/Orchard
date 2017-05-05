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

namespace Laser.Orchard.NwazetIntegration.Controllers {
    public class AddressesController : Controller {
        private readonly IOrderService _orderService;
        private readonly IPosServiceIntegration _posServiceIntegration;
        public AddressesController(IOrderService orderService, IPosServiceIntegration posServiceIntegration) {
            _orderService = orderService;
            _posServiceIntegration = posServiceIntegration;
        }
        [Themed]
        public ActionResult Index(AddressesVM model) {
            ActionResult result = null;
            OrderPart order = null;
            if(model.OrderId > 0) {
                order = _orderService.Get(model.OrderId);
            }
            switch (model.Submit) {
                case "cart":
                    result = RedirectToAction("Index", "ShoppingCart", new { area = "Nwazet.Commerce" });
                    break;
                case "save":
                    // Shipping Address
                    order.ShippingAddress.Address1 = model.ShippingAddress.Address1;
                    order.ShippingAddress.Address2 = model.ShippingAddress.Address2;
                    order.ShippingAddress.City = model.ShippingAddress.City;
                    order.ShippingAddress.Company = model.ShippingAddress.Company;
                    order.ShippingAddress.Country = model.ShippingAddress.Country;
                    order.ShippingAddress.FirstName = model.ShippingAddress.FirstName;
                    order.ShippingAddress.Honorific = model.ShippingAddress.Honorific;
                    order.ShippingAddress.LastName = model.ShippingAddress.LastName;
                    order.ShippingAddress.PostalCode = model.ShippingAddress.PostalCode;
                    order.ShippingAddress.Province = model.ShippingAddress.Province;
                    // Billing Address
                    order.BillingAddress.Address1 = model.BillingAddress.Address1;
                    order.BillingAddress.Address2 = model.BillingAddress.Address2;
                    order.BillingAddress.City = model.BillingAddress.City;
                    order.BillingAddress.Company = model.BillingAddress.Company;
                    order.BillingAddress.Country = model.BillingAddress.Country;
                    order.BillingAddress.FirstName = model.BillingAddress.FirstName;
                    order.BillingAddress.Honorific = model.BillingAddress.Honorific;
                    order.BillingAddress.LastName = model.BillingAddress.LastName;
                    order.BillingAddress.PostalCode = model.BillingAddress.PostalCode;
                    order.BillingAddress.Province = model.BillingAddress.Province;
                    //// other fields
                    //order.CustomerEmail = model.Email;
                    //order.CustomerPhone = model.Phone;
                    //order.SpecialInstructions = model.SpecialInstructions;

                    var reason = string.Format("Purchase Order kpo{0}", order.Id);
                    result = RedirectToAction("Pay", "Payment", new { area = "Laser.Orchard.PaymentGateway", reason =  reason, amount = order.Total, currency = order.CurrencyCode, itemId = order.Id});
                    break;
                default:
                    result = View("Index");
                    break;
            }
            return result;
        }
    }
}