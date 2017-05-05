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
        
        public ActionResult Index(AddressesVM model) {
            ActionResult result = null;
            OrderPart order = null;
            switch (model.Submit) {
                case "cart":
                    result = RedirectToAction("Index", "ShoppingCart", new { area = "Nwazet.Commerce" });
                    break;
                case "save":
                    if (model.OrderId > 0) {
                        order = _orderService.Get(model.OrderId);
                        order.ShippingAddress = model.ShippingAddress;
                        order.BillingAddress = model.BillingAddress;
                        //// other fields
                        //order.CustomerEmail = model.Email;
                        //order.CustomerPhone = model.Phone;
                        //order.SpecialInstructions = model.SpecialInstructions;

                        var reason = string.Format("Purchase Order kpo{0}", order.Id);
                        result = RedirectToAction("Pay", "Payment", new { area = "Laser.Orchard.PaymentGateway", reason = reason, amount = order.Total, currency = order.CurrencyCode, itemId = order.Id });
                    }
                    else {
                        result = RedirectToAction("Index", "ShoppingCart", new { area = "Nwazet.Commerce" });
                    }
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