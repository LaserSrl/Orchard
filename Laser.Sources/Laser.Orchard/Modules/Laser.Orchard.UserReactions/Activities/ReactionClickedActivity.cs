using Orchard.Localization;
using Orchard.Workflows.Activities;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.UserReactions.Activities {
    public class ReactionClickedActivity : Event {
        public Localizer T { get; set; }

        public ReactionClickedActivity() {
            T = NullLocalizer.Instance;
        }

        public override string Name {
            get { return "ReactionClicked"; }
        }

        public override LocalizedString Description {
            get { return T("A user reaction is clicked."); }
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(global::Orchard.Workflows.Models.WorkflowContext workflowContext, global::Orchard.Workflows.Models.ActivityContext activityContext) {
            return new[] { T("Clicked"), T("Unclicked"), T("NothingToDo") };
        }

        public override LocalizedString Category {
            get {
                return T("Social");
            }
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            LocalizedString messageout = null;
            var elencoTypeId = ((string)activityContext.GetState<string>("ReactionClickedActivity_reactionList")).Split(',').Select(Int32.Parse).ToList();
            int reactionId = Convert.ToInt32(workflowContext.Tokens["ReactionId"]);
            int action = Convert.ToInt32(workflowContext.Tokens["Action"]);
            workflowContext.SetState<int>("ReactionId", reactionId);
            workflowContext.SetState<int>("Action", action);
            if (elencoTypeId.Contains(reactionId)) {
                if (action == 1) {
                    messageout = T("Clicked");
                }
                else {
                    messageout = T("Unclicked");
                }
            }
            else {
                messageout = T("NothingToDo");
            }
            yield return messageout;
        }
        public override string Form {
            get {
                return "TheFormReactionClicked";
            }
        }

    }
}