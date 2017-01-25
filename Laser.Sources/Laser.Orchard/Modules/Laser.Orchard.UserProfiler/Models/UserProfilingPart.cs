using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.UserProfiler.Models {
    public class UserProfilingPart : ContentPart<UserProfilingPartRecord> {

    }

    public class UserProfilingPartRecord : ContentPartRecord {
        public UserProfilingPartRecord() {
            Summary = new List<UserProfilingSummaryRecord>();
        }

        public virtual IList<UserProfilingSummaryRecord> Summary { get; set; }
    }
}