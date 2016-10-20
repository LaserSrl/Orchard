using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Workflows.Activities;
using Orchard.Workflows.Services;
using Orchard.Logging;

namespace Laser.Orchard.StartupConfig.Services {
    public class ActivityServices : IActivityServices {
        private readonly IOrchardServices _orchardServices;
        private readonly IWorkflowManager _workflowManager;
        public Localizer T { get; set; }
        public ILogger Log{ get; set; }

        public ActivityServices(IOrchardServices orchardServices,
            IWorkflowManager workflowManager) {
            _orchardServices = orchardServices;
            T = NullLocalizer.Instance;
            _workflowManager = workflowManager;
            Log =  NullLogger.Instance;
        }

        public void TriggerSignal(string signalName, int contentId) {
            try {
                var content = _orchardServices.ContentManager.Get(contentId, VersionOptions.Published);
                var tokens = new Dictionary<string, object> { 
            { "Content", content }, 
            { SignalActivity.SignalEventName, signalName }};
                _workflowManager.TriggerEvent(SignalActivity.SignalEventName, content, () => tokens);
            } catch (Exception ex) {
                Log.Error("TriggerSignal "+ex.Message + "stack" + ex.StackTrace);
            }
        }
        public LocalizedString[] RequestInspectorWokflowOutcomes(string inspectionTypeString) {
            var inspectionType = InspectionType.DeviceBrand;
            Enum.TryParse(inspectionTypeString, out inspectionType);
            return RequestInspectorWokflowOutcomes(inspectionType);
        }
        public LocalizedString[] RequestInspectorWokflowOutcomes(InspectionType inspectionType) {
            if (inspectionType == InspectionType.Device) { } else if (inspectionType == InspectionType.DeviceBrand) {
                var strings = Enum.GetValues(typeof(DevicesBrands)).Cast<DevicesBrands>().Select(s=>T(s.ToString()));
                return strings.ToArray();
            }

            return new[] { T("Unknown") };

        }



    }
}