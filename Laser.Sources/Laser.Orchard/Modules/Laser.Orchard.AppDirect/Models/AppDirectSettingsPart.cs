using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace Laser.Orchard.AppDirect.Models {
    public class AppDirectSettingsPart : ContentPart<AppDirectSettingsPartRecord> {
        public string BaseUrl {
            get { return this.Retrieve(r => r.BaseUrl); }
            set { this.Store(r => r.BaseUrl, value); }
        }
    }
    public class AppDirectSettingsPartRecord: ContentPartRecord {
        public virtual string BaseUrl { get; set; }
    }
    public class AppDirectSettingsRecord {
        public virtual int Id { get; set; }
        public virtual string TheKey { get; set; }
        public virtual string ConsumerKey { get; set; }
        public virtual string ConsumerSecret { get; set; }
    }
}