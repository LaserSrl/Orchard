using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Mobile.Models {

    public class MobilePushPart : ContentPart<MobilePushPartRecord> {
        public string TitlePush {
            get { return Record.TitlePush; }
            set { Record.TitlePush = value; }
        }
        public string TextPush {
            get { return Record.TextPush; }
            set { Record.TextPush = value; }
        }
        public bool ToPush {
            get { return Record.ToPush; }
            set { Record.ToPush = value; }
        }
        public bool TestPush {
            get { return Record.TestPush; }
            set { Record.TestPush = value; }
        }
        public string DevicePush {
            get { return Record.DevicePush; }
            set { Record.DevicePush = value; }
        }

    }

    public class MobilePushPartRecord: ContentPartRecord {
        public virtual string TitlePush { get; set; }
        public virtual string TextPush { get; set; }
        public virtual bool ToPush { get; set; }
        public virtual bool TestPush { get; set; }
        public virtual string DevicePush { get; set; }
    }
}
