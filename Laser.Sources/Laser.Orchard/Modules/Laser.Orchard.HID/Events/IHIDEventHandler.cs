using Orchard.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laser.Orchard.HID.Events {
    public interface IHIDEventHandler : IEventHandler {

        /// <summary>
        /// Invoked when an HIDUser is created
        /// </summary>
        void HIDUserCreated(HIDEventContext context);

        /// <summary>
        /// Invoked when a credential is issued.
        /// </summary>
        void HIDCredentialIssued(HIDCredentialEventContext context);

        /// <summary>
        /// Invoked when a credential is revoked.
        /// </summary>
        void HIDCredentialRevoked(HIDCredentialEventContext context);
    }
}
