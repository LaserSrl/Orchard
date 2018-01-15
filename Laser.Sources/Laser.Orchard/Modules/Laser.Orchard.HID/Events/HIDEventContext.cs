using Laser.Orchard.HID.Models;

namespace Laser.Orchard.HID.Events {
    public class HIDEventContext {

        public HIDEventContext() { }

        public HIDEventContext(HIDUser user) {
            HIDUser = user;
        }

        public HIDUser HIDUser { get; set; }


    }
    
}