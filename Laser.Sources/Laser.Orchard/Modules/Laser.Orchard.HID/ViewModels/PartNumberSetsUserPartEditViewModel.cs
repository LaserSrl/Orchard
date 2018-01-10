using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.HID.ViewModels {
    public class PartNumberSetsUserPartEditViewModel {

        public PartNumberSetsUserPartEditViewModel() {

            Sets = new List<PartNumberSetsUserPartEditEntry>();
        }

        public IList<PartNumberSetsUserPartEditEntry> Sets { get; set; }
    }
}