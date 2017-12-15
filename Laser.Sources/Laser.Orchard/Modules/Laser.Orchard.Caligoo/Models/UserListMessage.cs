using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Laser.Orchard.Caligoo.Models {
    [DataContract]
    public class UserListMessage {
        [DataMember]
        public List<UserMessage> Data { get; set; }
        [DataMember]
        public string Previous { get; set; }
        [DataMember]
        public string Next { get; set; }
    }
}