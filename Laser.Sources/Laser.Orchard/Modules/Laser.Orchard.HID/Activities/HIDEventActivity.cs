using Orchard.Localization;
using Orchard.Workflows.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Workflows.Models;

namespace Laser.Orchard.HID.Activities {
    public abstract class HIDEventActivity : Event {

        protected HIDEventActivity() {

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override bool CanStartWorkflow {
            get { return true; }
        }

        public override bool CanExecute(WorkflowContext workflowContext, ActivityContext activityContext) {
            return true;
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            return new[] { T("Done") };
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            yield return T("Done");
        }

        public override LocalizedString Category {
            get { return T("Events"); }
        }

    }

    public class HIDUserCreatedActivity: HIDEventActivity {

        public override string Name {
            get { return "HIDUserCreated"; }
        }

        public override LocalizedString Description {
            get { return T("User is created."); }
        }
    }

    public class HIDCredentialIssuedActivity : HIDEventActivity {

        public override string Name {
            get { return "HIDCredentialIssued"; }
        }

        public override LocalizedString Description {
            get { return T("Credential is issued."); }
        }
    }

    public class HIDCredentialRevokedActivity : HIDEventActivity {

        public override string Name {
            get { return "HIDCredentialRevoked"; }
        }

        public override LocalizedString Description {
            get { return T("Credential is revoked."); }
        }
    }
}