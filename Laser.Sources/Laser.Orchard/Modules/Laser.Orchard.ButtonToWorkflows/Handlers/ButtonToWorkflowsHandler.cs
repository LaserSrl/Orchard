using Laser.Orchard.ButtonToWorkflows.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.UI.Notify;
using Orchard.Workflows.Services;
using System.Collections.Generic;

namespace Laser.Orchard.ButtonToWorkflows.Handlers {
    
    [OrchardFeature("Laser.Orchard.ButtonToWorkflows")]
    public class SiteSettingsPartHandler : ContentHandler {
        public SiteSettingsPartHandler(IRepository<ButtonToWorkflowsSettingsPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
            Filters.Add(new ActivatingFilter<ButtonToWorkflowsSettingsPart>("Site"));
            T = NullLocalizer.Instance;
            OnGetContentItemMetadata<ButtonToWorkflowsSettingsPart>((context, part) => context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("Buttons"))));
        }
        public Localizer T { get; set; }
    }

   [OrchardFeature("Laser.Orchard.ButtonToWorkflows")]
    public class ButtonToWorkflowsPartHandler : ContentHandler {
       public Localizer T { get; set; }
       private readonly IWorkflowManager _workflowManager;
       private readonly INotifier _notifier;
       public ButtonToWorkflowsPartHandler(IWorkflowManager workflowManager, INotifier notifier) {
           _workflowManager = workflowManager;
           _notifier = notifier;
           T = NullLocalizer.Instance;
           OnUpdated<ButtonToWorkflowsPart>((context, part) => {
               if (!string.IsNullOrEmpty(part.ActionToExecute)) {
                   var content = context.ContentItem;
                   _workflowManager.TriggerEvent(part.ActionToExecute, content, () => new Dictionary<string, object> { { "Content", content } });
                   try {
                       _notifier.Add(NotifyType.Information, T(part.MessageToWrite));
                   }
                   catch { }
                   part.MessageToWrite = "";
                   part.ActionToExecute = "";
               }
                   
               //  if (context.ContentItem.As<CommonPart>() != null) {
               //    var currentUser = _orchardServices.WorkContext.CurrentUser;
               //if (currentUser != null) {
               //    ((dynamic)context.ContentItem.As<CommonPart>()).LastModifier.Value = currentUser.Id;
               //    if (((dynamic)context.ContentItem.As<CommonPart>()).Creator.Value == null)
               //        //  ((NumericField) CommonPart.Fields.Where(x=>x.Name=="Creator").FirstOrDefault()).Value = currentUser.Id;
               //        ((dynamic)context.ContentItem.As<CommonPart>()).Creator.Value = currentUser.Id;
               //}
               //   }

           });
       }
    }
}