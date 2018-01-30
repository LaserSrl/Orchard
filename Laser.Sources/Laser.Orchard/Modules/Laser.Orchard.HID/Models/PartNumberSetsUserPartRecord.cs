using Orchard.ContentManagement.Records;
using System.Collections.Generic;

namespace Laser.Orchard.HID.Models {
    public class PartNumberSetsUserPartRecord : ContentPartRecord {

        public PartNumberSetsUserPartRecord() {
            PartNumberSetsJR = new List<PartNumberSetUserPartJunctionRecord>();
        }

        /// <summary>
        /// The selected HIDPartNumberSets
        /// </summary>
        public virtual IList<PartNumberSetUserPartJunctionRecord> PartNumberSetsJR { get; set; }
    }
}