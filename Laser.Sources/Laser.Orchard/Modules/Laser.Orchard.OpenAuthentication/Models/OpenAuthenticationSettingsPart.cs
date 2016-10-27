using Orchard.ContentManagement;

namespace Laser.Orchard.OpenAuthentication.Models {
    public class OpenAuthenticationSettingsPart : ContentPart<OpenAuthenticationSettingsPartRecord> {
        public bool AutoRegistrationEnabled {
            get { return Record.AutoRegistrationEnabled; }
            set { Record.AutoRegistrationEnabled = value; }
        }
    }
}