using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard;
using Orchard.Localization;

namespace Laser.Orchard.StartupConfig.Services {
    public interface IActivityServices : IDependency {
        void TriggerSignal(string signalName, int contentId);
        LocalizedString[] RequestInspectorWokflowOutcomes(InspectionType inspectionType);
        LocalizedString[] RequestInspectorWokflowOutcomes(string inspectionTypeString);
    }
}
