using System.Runtime.Serialization;

namespace Laser.Orchard.Caligoo.DataContracts {
    [DataContract]
    public class LocationMessage {
        [DataMember(Name = "location_id")]
        public string CaligooLocationId { get; set; }
        [DataMember(Name = "display_name")]
        public string DisplayName { get; set; }
        [DataMember(Name = "address")]
        public string Address { get; set; }
        [DataMember(Name = "postal_code")]
        public string PostalCode { get; set; }
        [DataMember(Name = "city")]
        public string City { get; set; }
        [DataMember(Name = "country")]
        public string Country { get; set; }
        [DataMember(Name = "geographic_location")]
        public GeographicLocationMessage GeographicLocation { get; set; }
    }
}