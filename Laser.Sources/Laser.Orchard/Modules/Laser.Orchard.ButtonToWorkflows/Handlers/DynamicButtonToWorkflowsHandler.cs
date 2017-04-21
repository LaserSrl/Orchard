using System;
using System.Collections.Generic;
using Laser.Orchard.ButtonToWorkflows.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Tasks.Scheduling;
using Orchard.UI.Notify;
using Orchard.Workflows.Services;

namespace Laser.Orchard.ButtonToWorkflows.Handlers {

    [OrchardFeature("Laser.Orchard.ButtonToWorkflows")]
    public class DynamicButtonToWorkflowsSettingsHandler : ContentHandler {

        public Localizer T { get; set; }

        public DynamicButtonToWorkflowsSettingsHandler() {
            T = NullLocalizer.Instance;
            Filters.Add(new ActivatingFilter<DynamicButtonToWorkflowsSettingsPart>("Site"));
        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            if (context.ContentItem.ContentType != "Site")
                return;

            base.GetItemMetadata(context);
            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("Buttons")));
        }
    }

    [OrchardFeature("Laser.Orchard.ButtonToWorkflows")]
    public class DynamicButtonToWorkflowsPartHandler : ContentHandler {

        private readonly INotifier _notifier;
        private readonly IScheduledTaskManager _scheduledTaskManager;
        private readonly IWorkflowManager _workflowManager;

        public Localizer T { get; set; }

        public DynamicButtonToWorkflowsPartHandler(INotifier notifier, IScheduledTaskManager scheduledTaskManager, IWorkflowManager workflowManager) {
            _notifier = notifier;
            _scheduledTaskManager = scheduledTaskManager;
            _workflowManager = workflowManager;
            T = NullLocalizer.Instance;

            OnUpdated<DynamicButtonToWorkflowsPart>((context, part) => {
                try {
                    var content = context.ContentItem;

                    if (part.ActionAsync) {
                        _scheduledTaskManager.CreateTask("Laser.Orchard.DynamicButtonToWorkflows.Task", DateTime.UtcNow.AddMinutes(1), part.ContentItem);
                    }
                    else {
                        _workflowManager.TriggerEvent("DynamicButtonEvent", content, () => new Dictionary<string, object> { { "ButtonName", part.ButtonName } });

                        part.ButtonName = "";
                        part.MessageToWrite = "";
                        part.ActionAsync = false;
                    }

                    if (!string.IsNullOrEmpty(part.MessageToWrite))
                        _notifier.Add(NotifyType.Information, T(part.MessageToWrite));
                }
                catch (Exception ex) {
                    Logger.Error(ex, "Error in DynamicButtonToWorkflowsPartHandler. ContentItem: {0}", context.ContentItem);

                    part.ButtonName = "";
                    part.MessageToWrite = "";
                    part.ActionAsync = false;
                }
            });
        }
    }
}