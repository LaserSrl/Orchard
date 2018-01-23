using System.Runtime.Serialization;

namespace Laser.Orchard.Caligoo.DataContracts {
    [DataContract]
    public class KeyValueMessage {
        [DataMember(Name = "key")]
        public string Key { get; set; }
        [DataMember(Name = "value")]
        public object Value { get; set; }
    }
}