using Nwazet.Commerce.Models;

namespace Laser.Orchard.NwazetIntegration.Models {
    public class KrakePaymentCharge : ICharge {
        private string _text;
        public KrakePaymentCharge(string text = "Krake payment") {
            _text = text;
        }
        public string ChargeText
        {
            get
            {
                return _text;
            }
        }
        public CheckoutError Error { get; set; }
        public string TransactionId { get; set; }
    }
}