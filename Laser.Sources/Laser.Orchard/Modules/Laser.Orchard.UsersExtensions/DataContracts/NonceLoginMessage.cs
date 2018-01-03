using System.Runtime.Serialization;

namespace Laser.Orchard.UsersExtensions.DataContracts {
    [DataContract]
    public class NonceLoginMessage {
        [DataMember(Name = "nonce")]
        public string Nonce { get; set; }
    }
}