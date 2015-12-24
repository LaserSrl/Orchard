using System;
using Orchard.ContentManagement.Records;

namespace Laser.Orchard.SEO.Models {
    [Obsolete("Replaced with 'SeoVersionRecord' to enable versioning of this content")]
    public class SeoRecord : ContentPartRecord {
        public virtual string TitleOverride { get; set; }
        public virtual string Keywords { get; set; }
        public virtual string Description { get; set; }
    }

    public class SeoVersionRecord : ContentPartVersionRecord {
        public virtual string TitleOverride { get; set; }
        public virtual string Keywords { get; set; }
        public virtual string Description { get; set; }
    }

}