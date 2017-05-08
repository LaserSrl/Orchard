using Nwazet.Commerce.Models;

namespace Laser.Orchard.NwazetIntegration.Models {
    public class KrakePaymentCharge : ICharge {
        private string _text;
        private string _transactionId;
        public KrakePaymentCharge(string text = "Krake payment", string transactionId = "") {
            _text = text;
            _transactionId = transactionId;
        }
        public string ChargeText
        {
            get
            {
                return _text;
            }
        }
        public CheckoutError Error { get; set; }
        public string TransactionId
        {
            get
            {
                return _transactionId;
            }
        }
    }
}