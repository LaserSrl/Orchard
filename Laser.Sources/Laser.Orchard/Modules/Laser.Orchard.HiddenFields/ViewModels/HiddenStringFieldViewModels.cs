using Laser.Orchard.HiddenFields.Fields;
using Laser.Orchard.HiddenFields.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.HiddenFields.ViewModels {
    //As of 20160622, this class simply wraps HiddenStringFieldSettings. However, it is already provided as a View Model
    //to allow adding things to it while limiting required code changes.
    public class HiddenStringFieldSettingsEventsViewModel {

        public HiddenStringFieldSettings Settings { get; set; }
    }

    public class HiddenStringFieldDriverViewModel {

        public bool IsEditAuthorized { get; set; }
        
        public HiddenStringField Field { get; set; }

        public string Value { get; set; }

        public HiddenStringFieldSettings Settings { get; set; }
    }
}