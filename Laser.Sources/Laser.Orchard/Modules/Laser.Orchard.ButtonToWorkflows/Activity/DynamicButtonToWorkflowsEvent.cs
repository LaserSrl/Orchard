using System.Collections.Generic;
using System.Linq;
using Laser.Orchard.ButtonToWorkflows.Services;
using Orchard.Localization;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;

namespace Laser.Orchard.ButtonToWorkflows.Activity {
    public class DynamicButtonToWorkflowsEvent : Event {

        private readonly IDynamicButtonToWorkflowsService _dynamicButtonToWorkflowsService;
        public Localizer T { get; set; }

        public DynamicButtonToWorkflowsEvent(IDynamicButtonToWorkflowsService dynamicButtonToWorkflowsService) {
            _dynamicButtonToWorkflowsService = dynamicButtonToWorkflowsService;
            T = NullLocalizer.Instance;
        }

        public override bool CanStartWorkflow
        {
            get { return true; }
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            return new[] { T("Done") };
        }

        public override bool CanExecute(WorkflowContext workflowContext, ActivityContext activityContext) {
            try {
                var buttonSelected = activityContext.GetState<string>("DynamicButton");

                if (!string.IsNullOrWhiteSpace(buttonSelected) && workflowContext.Tokens.ContainsKey("ButtonName")) {
                    var clickedButtonName = workflowContext.Tokens["ButtonName"].ToString();

                    if (!string.IsNullOrWhiteSpace(clickedButtonName)) {
                        var clickedButtonId = _dynamicButtonToWorkflowsService.GetButtons().Where(w => w.ButtonName == clickedButtonName).Select(s => s.Id).FirstOrDefault();
                        return clickedButtonId.ToString() == buttonSelected;
                    }
                }

                return false;
            }
            catch {
                return false;
            }
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            yield return T("Done");
        }

        public override LocalizedString Category
        {
            get { return T("Content Event"); }
        }

        public override string Name
        {
            get { return "DynamicButtonEvent"; }
        }

        public override string Form
        {
            get { return "_DynamicButtonSelectForm"; }
        }

        public override LocalizedString Description
        {
            get { return T("Dynamic button is clicked."); }
        }
    }
}