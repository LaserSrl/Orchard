using System;
using System.Collections.Generic;
using Orchard.Logging;
using Orchard.Tasks.Scheduling;
using Orchard.Workflows.Services;

namespace Laser.Orchard.AppDirect.Handlers {
    public class AppDirectTaskScheduleHandler : IScheduledTaskHandler {
        private readonly IWorkflowManager _workflowManager;
        private const string TaskType = "Laser.Orchard.AppDirect.Task";
        public ILogger Logger { get; set; }
        public AppDirectTaskScheduleHandler(IWorkflowManager workflowManager) {
            Logger = NullLogger.Instance;
            _workflowManager = workflowManager;
        }

        public void Process(ScheduledTaskContext context) {
            try {
                if (context.Task.TaskType != TaskType) {
                    return;
                }
                var action = "";
                string val = ((dynamic)context.Task.ContentItem).AppDirectRequestPart.State.Value.ToString();
                switch (val) {
                    case "ToCreate":
                        action = "Create";
                        break;
                    case "ToCancel":
                        action = "Cancel";
                        break;
                    case "Edit":
                        action = "Edit";
                        break;
                    case "ToAssignUser":
                        action = "AssignUser";
                        break;
                    case "ToUnAssignUser":
                        action = "UnAssignUser";
                        break;
                    case "Status":
                        action = "Status";
                        break;
                }
                _workflowManager.TriggerEvent("SubscriptionEvent", context.Task.ContentItem, () => new Dictionary<string, object> { { "Content", context.Task.ContentItem }, { "Action", action } });
            }
            catch (Exception ex) {
                Logger.Error(ex, "Error in AppDirectTaskScheduleHandler. ContentItem: {0}, ScheduledUtc: {1:yyyy-MM-dd HH.mm.ss}", context.Task.ContentItem, context.Task.ScheduledUtc);
            }
        }
    }
}