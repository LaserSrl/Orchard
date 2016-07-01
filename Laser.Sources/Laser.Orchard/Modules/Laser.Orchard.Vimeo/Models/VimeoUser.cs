using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Vimeo.Models {
    public class VimeoUser : VimeoBaseObject {
        public string location { get; set; }
        public string bio { get; set; }
        public string account { get; set; }
        public List<VimeoWebsite> websites { get; set; }
        public VimeoUserMetadata metadata { get; set; }
        public VimeoUserPreferences preferences { get; set; }
        public List<string> contentFilter { get; set; }
        public string resourceKey { get; set; }
        public VimeoUploadQuota upload_quota { get; set; }
    }

    public class VimeoUserMetadata : VimeoBaseMetadata {

    }

    public class VimeoUserPreferences {
        public Dictionary<string, string> videos { get; set; }
    }

    public class VimeoUploadQuota {
        public VimeoSpace space { get; set; }
        public VimeoQuota quota { get; set; }

        public VimeoUploadQuota() {
            space = new VimeoSpace();
            quota = new VimeoQuota();
        }
    }
    public class VimeoSpace {
        public int free { get; set; }
        public int max { get; set; }
        public int used { get; set; }
    }
    public class VimeoQuota {
        public bool hd { get; set; }
        public bool sd { get; set; }
    }
}