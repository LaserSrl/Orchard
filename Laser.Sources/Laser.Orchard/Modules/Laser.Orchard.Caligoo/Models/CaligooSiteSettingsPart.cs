using Orchard.ContentManagement;

namespace Laser.Orchard.Caligoo.Models {
    public class CaligooSiteSettingsPart : ContentPart {
        public string LoginUrl {
            get { return this.Retrieve(x => x.LoginUrl); }
            set { this.Store(x => x.LoginUrl, value); }
        }
        /// <summary>
        /// Timeout in milliseconds during requests to Caligoo Web API.
        /// </summary>
        public int RequestTimeoutMillis {
            get { return this.Retrieve(x => x.RequestTimeoutMillis); }
            set { this.Store(x => x.RequestTimeoutMillis, value); }
        }
    }
}