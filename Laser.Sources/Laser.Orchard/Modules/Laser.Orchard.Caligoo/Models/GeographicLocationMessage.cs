using System.Runtime.Serialization;

namespace Laser.Orchard.Caligoo.Models {
    [DataContract]
    public class GeographicLocationMessage {
        [DataMember(Name = "lat")]
        public decimal Latitude { get; set; }
        [DataMember(Name = "lng")]
        public decimal Longitude { get; set; }
    }
}