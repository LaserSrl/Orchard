using Newtonsoft.Json.Linq;

namespace Laser.Orchard.HID.Models {
    public class HIDCredential {
        public int Id { get; set; } //id of credential in HID systems
        public string PartNumber { get; set; }
        public string PartNumberFriendlyName { get; set; }
        public string CardNumber { get; set; } //Access Control Number
        public string Status { get; set; }

        public HIDCredential() { }

        public HIDCredential(JToken credential)
            : this() {
            Id = int.Parse(credential["id"].ToString());
            PartNumber = credential["partNumber"].ToString();
            PartNumberFriendlyName = credential["partnumberFriendlyName"].ToString();
            CardNumber = credential["cardNumber"].ToString();
            Status = credential["status"].ToString().ToUpperInvariant();
        }
    }
}