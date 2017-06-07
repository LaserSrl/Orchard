using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace Laser.Orchard.DataProtection.Models {
    public class DataContextPart : ContentPart<DataContextPartRecord> {
        public string Context {
            get { return Retrieve(r => r.Context); }
            set { Store(r => r.Context, value); }
        }
    }

    public class DataContextPartRecord : ContentPartVersionRecord {
        public virtual string Context { get; set; }
    }
}