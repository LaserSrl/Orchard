using Laser.Orchard.HID.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.HID.Events {
    public class HIDEventContext {

        public HIDEventContext() { }

        public HIDEventContext(HIDUser user) {
            HIDUser = user;
        }

        public HIDUser HIDUser { get; set; }


    }
    
}