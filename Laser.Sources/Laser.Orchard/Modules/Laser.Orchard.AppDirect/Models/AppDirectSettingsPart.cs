using Orchard.ContentManagement;

namespace Laser.Orchard.AppDirect.Models {
    public class AppDirectSettingsPart : ContentPart { }
    public class AppDirectSettingsRecord {
        public virtual int Id { get; set; }
        public virtual string TheKey { get; set; }
        public virtual string ConsumerKey { get; set; }
        public virtual string ConsumerSecret { get; set; }
    }
}