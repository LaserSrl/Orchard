using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.AppDirect.Models;
using Orchard.Logging;
using Orchard.Tasks.Scheduling;
using Orchard.Workflows.Services;

namespace Laser.Orchard.AppDirect.Handlers {
    public class AppDirectActionExecutedScheduleHandler : IScheduledTaskHandler {
        private readonly IWorkflowManager _workflowManager;
        private const string TaskType = "Laser.Orchard.AppDirect.ActionExecuted";
        public ILogger Logger { get; set; }
        public AppDirectActionExecutedScheduleHandler(IWorkflowManager workflowManager) {
            Logger = NullLogger.Instance;
            _workflowManager = workflowManager;
        }

        public void Process(ScheduledTaskContext context) {
            try {
                if (context.Task.TaskType != TaskType) {
                    return;
                }
                string action = ((dynamic)context.Task.ContentItem).AppDirectRequestPart.State.Value.ToString();
                _workflowManager.TriggerEvent("SubscriptionActionExecutedEvent", context.Task.ContentItem, () => new Dictionary<string, object> { { "Content", context.Task.ContentItem }, { "Action", action } });
            }
            catch (Exception ex) {
                Logger.Error(ex, "Error in AppDirectActionExecutedScheduleHandler. ContentItem: {0}, ScheduledUtc: {1:yyyy-MM-dd HH.mm.ss}", context.Task.ContentItem, context.Task.ScheduledUtc);
            }
        }
    }
}