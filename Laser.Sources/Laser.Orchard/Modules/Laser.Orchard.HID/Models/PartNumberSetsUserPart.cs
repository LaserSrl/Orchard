using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.HID.Models {
    /// <summary>
    /// This part is used to relate a User to HIDPartNumberSets 
    /// </summary>
    public class PartNumberSetsUserPart : ContentPart<PartNumberSetsUserPartRecord> {

        public IEnumerable<HIDPartNumberSet> PartNumberSets {
            get { return Record.PartNumberSets; }
        }
        
    }
}