using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.ButtonToWorkflows.Models {
    public class ButtonToWorkflowsPart : ContentPart<ButtonToWorkflowsPartRecord> {
        public string FromUser {
            get { return this.Retrieve(r => r.FromUser); }
            set { this.Store(r => r.FromUser, value); }
        }
        public string ToUser {
            get { return this.Retrieve(r => r.ToUser); }
            set { this.Store(r => r.ToUser, value); }
        }
        public int FromIdUser {
            get { return this.Retrieve(r => r.FromIdUser); }
            set { this.Store(r => r.FromIdUser, value); }
        }
        public int ToIdUser {
            get { return this.Retrieve(r => r.ToIdUser); }
            set { this.Store(r => r.ToIdUser, value); }
        }
    }
    public class ButtonToWorkflowsPartRecord: ContentPartRecord{
        public virtual string FromUser { get; set; }
        public virtual string ToUser { get; set; }
        public virtual int FromIdUser { get; set; }
        public virtual int ToIdUser { get; set; }
    }
}