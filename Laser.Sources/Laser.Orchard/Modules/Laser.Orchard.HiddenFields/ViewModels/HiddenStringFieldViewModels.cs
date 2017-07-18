using Laser.Orchard.HiddenFields.Fields;
using Laser.Orchard.HiddenFields.Services;
using Laser.Orchard.HiddenFields.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.HiddenFields.ViewModels {

    public class HiddenStringFieldSettingsEventsViewModel {

        public HiddenStringFieldSettings Settings { get; set; }

        public HiddenStringFieldUpdateProcessVariant ProcessVariant {get;set;}
    }

    public class HiddenStringFieldDriverViewModel {

        public bool IsEditAuthorized { get; set; }
        
        public HiddenStringField Field { get; set; }

        public string Value { get; set; }

        public HiddenStringFieldSettings Settings { get; set; }
    }
}