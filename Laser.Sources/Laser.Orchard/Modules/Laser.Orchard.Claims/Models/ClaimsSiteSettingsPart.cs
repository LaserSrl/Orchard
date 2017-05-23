using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Claims.Models {
    public class ClaimsSiteSettingsPart : ContentPart {
        public bool ApplyToFrontEnd {
            get { return this.Retrieve(x => x.ApplyToFrontEnd); }
            set { this.Store(x => x.ApplyToFrontEnd, value); }
        }
    }
}