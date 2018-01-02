using System.Runtime.Serialization;

namespace Laser.Orchard.UsersExtensions.DataContracts {
    [DataContract]
    public class NonceLoginMessage {
        [DataMember(Name = "mail")]
        public string Mail { get; set; }
        [DataMember(Name = "nonce")]
        public string Nonce { get; set; }
    }
}