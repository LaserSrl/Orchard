using System.Collections.Generic;
using Laser.Orchard.AppDirect.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;

namespace Laser.Orchard.AppDirect.Activity {
    public class SubscriptionActionExecutedEvent : Event {
        protected readonly IOrchardServices _orchardServices;
        private readonly IContentManager _contentManager;
        public SubscriptionActionExecutedEvent(IOrchardServices orchardServices, IContentManager contentManager) {
            _orchardServices = orchardServices;
            T = NullLocalizer.Instance;
            _contentManager = contentManager;
        }
        public Localizer T { get; set; }

        public override bool CanStartWorkflow {
            get { return true; }
        }

        public override string Name {
            get
            {
                return nameof(SubscriptionActionExecutedEvent);
            }
        }

        public override LocalizedString Category {
            get
            {
                return T("Content");// throw new NotImplementedException();
            }
        }

        public override LocalizedString Description {
            get
            {
                return T("Manage AppMarket Subscription Action Executed Event");
            }
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            return new[] { T(RequestState.Created.ToString()), T(RequestState.Modified.ToString()), T(RequestState.Cancelled.ToString()), T(RequestState.AssignedUser.ToString()), T(RequestState.UnAssignedUser.ToString()) };
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            var operatore = workflowContext.Tokens["Action"].ToString();
            yield return T(operatore);
        }
    }
}