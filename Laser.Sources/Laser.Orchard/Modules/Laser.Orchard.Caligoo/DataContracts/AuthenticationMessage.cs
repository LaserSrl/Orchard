using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Laser.Orchard.Caligoo.DataContracts {
    [DataContract]
    public class AuthenticationMessage {
        [DataMember(Name = "status")]
        public bool Status { get; set; }
        [DataMember(Name = "consumer_name")]
        public string ConsumerName { get; set; }
        [DataMember(Name = "consumer_id")]
        public string ConsumerId { get; set; }
        [DataMember(Name = "credential_username")]
        public string Username { get; set; }
        [DataMember(Name = "expires")]
        public decimal Expires { get; set; }
        [DataMember(Name = "duration")]
        public string Duration { get; set; }
        [DataMember(Name = "token")]
        public string Token { get; set; }
        [DataMember(Name = "authorization")]
        public string Bearer { get; set; }
        [DataMember(Name = "message")]
        public string Message { get; set; }
    }
}