using Orchard.ContentManagement.Records;

namespace Laser.Orchard.OpenAuthentication.Models {
    public class OpenAuthenticationSettingsPartRecord : ContentPartRecord {
        public virtual bool AutoRegistrationEnabled { get; set; }
    }
}