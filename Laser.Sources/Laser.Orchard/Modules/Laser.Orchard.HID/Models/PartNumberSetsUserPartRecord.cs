using Orchard.ContentManagement.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.HID.Models {
    public class PartNumberSetsUserPartRecord : ContentPartRecord {

        public PartNumberSetsUserPartRecord() {
            PartNumberSets = new List<HIDPartNumberSet>();
        }

        /// <summary>
        /// The selected HIDPartNumberSets
        /// </summary>
        public virtual IList<HIDPartNumberSet> PartNumberSets { get; set; }
    }
}