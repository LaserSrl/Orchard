using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace Laser.Orchard.Caligoo.Models {
    public class CaligooUserPart : ContentPart<CaligooUserPartRecord> {
        public string CaligooUserId {
            get { return Retrieve(r => r.CaligooUserId); }
            set { Store(r => r.CaligooUserId, value); }
        }
        public string CaligooUserName {
            get { return Retrieve(r => r.CaligooUserName); }
            set { Store(r => r.CaligooUserName, value); }
        }
    }

    public class CaligooUserPartRecord : ContentPartRecord {
        public virtual string CaligooUserId { get; set; }
        public virtual string CaligooUserName { get; set; }
    }
}