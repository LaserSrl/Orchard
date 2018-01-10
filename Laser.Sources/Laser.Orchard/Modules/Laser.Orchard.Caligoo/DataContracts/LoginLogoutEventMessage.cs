using System;
using System.Runtime.Serialization;

namespace Laser.Orchard.Caligoo.DataContracts {
    [DataContract]
    public class LoginLogoutEventMessage {
        [DataMember(Name = "event_type")]
        public string EventType { get; set; }
        [DataMember(Name = "new")]
        public bool New { get; set; }
        [DataMember(Name = "user_id")]
        public string CaligooUserId { get; set; }
        [DataMember(Name = "location_id")]
        public string CaligooLocationId { get; set; }
        [DataMember(Name = "start_timestamp")]
        public int StartTimestamp { get; set; }
        [DataMember(Name = "stop_timestamp")]
        public int StopTimestamp { get; set; }
        public DateTime StartDateTime {
            get {
                return new DateTime(1970, 1, 1).AddSeconds(StartTimestamp);
            }
        }
        public DateTime StopDateTime {
            get {
                return new DateTime(1970, 1, 1).AddSeconds(StopTimestamp);
            }
        }
    }
}