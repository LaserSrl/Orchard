using Orchard.Environment.Extensions;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.ViewModels {
    [OrchardFeature("Laser.Orchard.StartupConfig.Maintenance")]
    public class MaintenanceVM {
        public NotifyType MaintenanceNotifyType { get; set; }
        public string MaintenanceNotify { get; set; }
    }
}