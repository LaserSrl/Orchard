﻿using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace Laser.Orchard.Caligoo.Models {
    public class CaligooUserPart : ContentPart<CaligooUserPartRecord> {
        public string CaligooUserId {
            get { return Retrieve(r => r.CaligooUserId); }
            set { Store(r => r.CaligooUserId, value); }
        }
    }

    public class CaligooUserPartRecord : ContentPartRecord {
        public virtual string CaligooUserId { get; set; }
    }
}