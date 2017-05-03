using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.PaymentGateway.Models;
using Laser.Orchard.PaymentGateway.ViewModels;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Orchard.DisplayManagement;

namespace Laser.Orchard.NwazetIntegration.Services {
    public interface IPosServiceIntegration : ICheckoutService {
    }

    public class PosServiceIntegration : IPosServiceIntegration {
        private readonly IEnumerable<IPosService> _posServices;
        private readonly dynamic _shapeFactory;

        public PosServiceIntegration(IEnumerable<IPosService> posServices, IShapeFactory shapeFactory) {
            _posServices = posServices;
            _shapeFactory = shapeFactory;
        }

        public string Name
        {
            get
            {
                return "Krake payments";
            }
        }

        public dynamic BuildCheckoutButtonShape(IEnumerable<dynamic> productShapes, IEnumerable<ShoppingCartQuantityProduct> productQuantities, IEnumerable<ShippingOption> shippingOptions, TaxAmount taxes, string country, string zipCode, IEnumerable<string> custom) {
            //throw new NotImplementedException();
            decimal amount = 100;

            PaymentVM model = new PaymentVM {
                Record = new PaymentRecord {
                    Reason = "Buy products",
                    Amount = amount,
                    Currency = "EUR",
                    ContentItemId = 0
                },
                PosList = _posServices.ToList(),
                ContentItem = null
            };
            return _shapeFactory.Pos(model);
        }

        public string GetChargeAdminUrl(string transactionId) {
            return _posServices.ElementAt(0).GetPosActionUrl(transactionId);
        }
    }
}