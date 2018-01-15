using Laser.Orchard.HID.Events;
using Orchard.Workflows.Services;
using System.Collections.Generic;

namespace Laser.Orchard.HID.Handlers {
    public class HIDWorkflowEventHandler : IHIDEventHandler {

        private readonly IWorkflowManager _workflowManager;

        public HIDWorkflowEventHandler(
            IWorkflowManager workflowManager) {

            _workflowManager = workflowManager;
        }

        public void HIDCredentialIssued(HIDCredentialEventContext context) {
            _workflowManager.TriggerEvent("HIDCredentialIssued",
                null,
                () => new Dictionary<string, object> {
                    {"HIDUser", context.HIDUser},
                    { "PartNumber", context.PartNumber }
                });
        }

        public void HIDCredentialRevoked(HIDCredentialEventContext context) {
            _workflowManager.TriggerEvent("HIDCredentialRevoked",
                null,
                () => new Dictionary<string, object> {
                    {"HIDUser", context.HIDUser},
                    { "PartNumber", context.PartNumber }
                });
        }

        public void HIDUserCreated(HIDEventContext context) {
            _workflowManager.TriggerEvent("UserCreated",
                null,
                () => new Dictionary<string, object> {
                    {"HIDUser", context.HIDUser}
                });
        }
    }
}