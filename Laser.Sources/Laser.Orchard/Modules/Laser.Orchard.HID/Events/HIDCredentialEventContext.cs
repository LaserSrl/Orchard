using Laser.Orchard.HID.Models;
using System.Collections.Generic;

namespace Laser.Orchard.HID.Events {
    public class HIDCredentialEventContext : HIDEventContext {

        public HIDCredentialEventContext() : base() {
            AffectedCredentials = new List<HIDCredential>();
        }

        public HIDCredentialEventContext(HIDUser user) : base(user) {
            AffectedCredentials = new List<HIDCredential>();
        }

        public HIDCredentialEventContext(string pn) : base() {
            AffectedCredentials = new List<HIDCredential>();
            PartNumber = pn;
        }

        public HIDCredentialEventContext(HIDUser user, string pn) : base(user) {
            AffectedCredentials = new List<HIDCredential>();
            PartNumber = pn;
        }

        public string PartNumber { get; set; }

        // If we know the CredentialContainer we worked on, and given the Part Number, we 
        // can easily figure out the HIDCredential object affected. If we are trying to work
        // with the default PartNumber we probably don't have information on it here, meaning
        // we don't know what is the actual PartNumber. In that case, knowing the Container is
        // not enough to find the HIDCredential affected.
        // On the other hand, given only the HIDUser and the PartNumber we cannot be sure even
        // of which is the Credential Container, because that PartNumber may be present on more
        // than one.
        // It would then appear that the only way to be sure about which is the HIDCredential
        // affected in the event is to pass it along explicitly, leaving the service where the
        // event is triggered with the duty of identifying it.
        // We use an IEnumerable rather than a single object because an event may affect several
        // credentials.
        // Since we have the HIDUser, it's easy to find the corresponding CredentialContainer, by
        // searching the one that contains the Credential.
        public IList<HIDCredential> AffectedCredentials { get; set; }
    }
}