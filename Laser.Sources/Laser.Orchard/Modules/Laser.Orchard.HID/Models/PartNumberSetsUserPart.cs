using Orchard.ContentManagement;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.HID.Models {
    /// <summary>
    /// This part is used to relate a User to HIDPartNumberSets 
    /// </summary>
    public class PartNumberSetsUserPart : ContentPart<PartNumberSetsUserPartRecord> {

        public IEnumerable<HIDPartNumberSet> PartNumberSets {
            get { return Record.PartNumberSetsJR.Select(jr => jr.HIDPartNumberSet); }
        }

        //private readonly LazyField<IEnumerable<HIDPartNumberSet>> _partNumberSets =
        //    new LazyField<IEnumerable<HIDPartNumberSet>>();

        //public LazyField<IEnumerable<HIDPartNumberSet>> PartNumberSetsField {
        //    get { return _partNumberSets; }
        //}

        //public IEnumerable<HIDPartNumberSet> PartNumberSets {
        //    get { return _partNumberSets.Value; }
        //}
    }
}