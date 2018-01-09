using Orchard.ContentManagement;

namespace Laser.Orchard.UsersExtensions.Models {
    public class NonceLoginSettingsPart : ContentPart {
        public int NonceMinutesValidity {
            get { return this.Retrieve(x => x.NonceMinutesValidity); }
            set { this.Store(x => x.NonceMinutesValidity, value); }
        }
        public string LoginLinkFormat {
            get { return this.Retrieve(x => x.LoginLinkFormat); }
            set { this.Store(x => x.LoginLinkFormat, value); }
        }
    }
}