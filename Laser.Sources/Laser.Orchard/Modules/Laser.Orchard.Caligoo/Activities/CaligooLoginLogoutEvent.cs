using Orchard.Localization;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;
using System;
using System.Collections.Generic;

namespace Laser.Orchard.Caligoo.Activities {
    public class CaligooLoginLogoutEvent : Event {
        public Localizer T { get; set; }
        public CaligooLoginLogoutEvent() {
            T = NullLocalizer.Instance;
        }
        public override bool CanStartWorkflow {
            get { return true; }
        }
        public override LocalizedString Category {
            get { return T("Content Event"); }
        }

        public override LocalizedString Description {
            get { return T("A contact has just performed login or logout from Caligoo network."); }
        }

        public override string Name {
            get { return "CaligooLoginLogoutEvent"; }
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            var result = T("");
            var eventType = workflowContext.Tokens["Event"].ToString();
            switch (eventType) {
                case "login":
                    result = T("Login");
                    break;
                case "logout":
                    result = T("Logout");
                    break;
                default:
                    throw new Exception(string.Format("Unexpected Event token value: {0}", eventType));
            }
            yield return result;
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            return new[] { T("Login"), T("Logout") };
        }
    }
}