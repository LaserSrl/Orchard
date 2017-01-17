using Newtonsoft.Json.Linq;

namespace Laser.Orchard.HID.Models {
    public class HIDCredential {
        public int Id { get; private set; } //id of credential in HID systems
        public string PartNumber { get; private set; }
        public string PartNumberFriendlyName { get; private set; }
        public string CardNumber { get; private set; } //Access Control Number
        public string Status { get; set; }

        private HIDCredential() { }

        public HIDCredential(JToken credential)
            : this() {
            Id = int.Parse(credential["id"].ToString());
            PartNumber = credential["partNumber"].ToString();
            PartNumberFriendlyName = credential["partnumberFriendlyName"].ToString();
            CardNumber = credential["cardNumber"].ToString();
            Status = credential["status"].ToString();
        }
    }
}