using Laser.Orchard.HID.Models;
using Orchard.Security;

namespace Laser.Orchard.HID.Events {
    public class HIDEventContext {

        public HIDEventContext() { }

        public HIDEventContext(HIDUser user) {
            HIDUser = user;
        }

        public HIDUser HIDUser { get; set; }

        public IUser User { get; set; }
    }
    
}