using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace Laser.Orchard.AppDirect.Models {
    public class AppDirectSettingsPart : ContentPart { }
    //public class AppDirectSettingsPart : ContentPart<AppDirectSettingsPartRecord> {
    //    public string ConsumerKey {
    //        get
    //        {
    //            return this.Retrieve(r => r.ConsumerKey);
    //        }
    //        set
    //        {
    //            this.Store(r => r.ConsumerKey, value);
    //        }
    //    }
    //    public string ConsumerSecret {
    //        get
    //        {
    //            return this.Retrieve(r => r.ConsumerSecret);
    //        }
    //        set
    //        {
    //            this.Store(r => r.ConsumerSecret, value);
    //        }
    //    }
    //}
    //public class AppDirectSettingsPartRecord : ContentPartRecord {
    //    public virtual string ConsumerKey { get; set; }
    //    public virtual string ConsumerSecret { get; set; }
    //}

    public class AppDirectSettingsRecord {
        public virtual int Id { get; set; }
        public virtual string TheKey { get; set; }
        public virtual string ConsumerKey { get; set; }
        public virtual string ConsumerSecret { get; set; }
    }
}