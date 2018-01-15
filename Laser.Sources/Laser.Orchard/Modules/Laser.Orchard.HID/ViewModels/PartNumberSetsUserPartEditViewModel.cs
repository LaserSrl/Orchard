using System.Collections.Generic;

namespace Laser.Orchard.HID.ViewModels {
    public class PartNumberSetsUserPartEditViewModel {

        public PartNumberSetsUserPartEditViewModel() {

            Sets = new List<PartNumberSetsUserPartEditEntry>();
        }

        public IList<PartNumberSetsUserPartEditEntry> Sets { get; set; }
    }
}