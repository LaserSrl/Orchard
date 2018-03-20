using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Laser.Orchard.Caligoo.DataContracts {
    [DataContract]
    public class UserMessage {
        [DataMember(Name = "user_id")]
        public string CaligooUserId { get; set; }
        [DataMember(Name = "username")]
        public string CaligooUserName { get; set; }
        [DataMember(Name = "additionalInfo")]
        public List<KeyValueMessage> AdditionalInfo { get; set; }
    }
}