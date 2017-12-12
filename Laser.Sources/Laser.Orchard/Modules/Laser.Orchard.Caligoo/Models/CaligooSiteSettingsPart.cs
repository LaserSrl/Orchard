using Orchard.ContentManagement;

namespace Laser.Orchard.Caligoo.Models {
    public class CaligooSiteSettingsPart : ContentPart {
        public string LoginUrl {
            get { return this.Retrieve(XmlHelper => XmlHelper.LoginUrl); }
            set { this.Store(x => x.LoginUrl, value); }
        }
    }
}