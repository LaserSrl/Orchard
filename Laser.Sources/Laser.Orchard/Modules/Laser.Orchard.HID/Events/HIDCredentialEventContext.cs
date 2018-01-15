using Laser.Orchard.HID.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.HID.Events {
    public class HIDCredentialEventContext : HIDEventContext {

        public HIDCredentialEventContext() : base() { }

        public HIDCredentialEventContext(HIDUser user) : base(user) { }

        public HIDCredentialEventContext(string pn) : base() {
            PartNumber = pn;
        }

        public HIDCredentialEventContext(HIDUser user, string pn) : base(user) {
            PartNumber = pn;
        }

        public string PartNumber { get; set; }
    }
}