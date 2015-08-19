using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace Laser.Orchard.Policy.ViewModels {
    public class PoliciesApiModel {
        public string Language { get; set; }
        public SimplePoliciesForUserViewModel PoliciesForUser { get; set; }
    }
    
    public class PoliciesForUserViewModel {
        public PoliciesForUserViewModel() {
            Policies = new List<PolicyForUserViewModel>();
        }
        public IList<PolicyForUserViewModel> Policies{ get; set; }
    }

    public class SimplePoliciesForUserViewModel {
        public SimplePoliciesForUserViewModel() {
            Policies = new List<SimplePolicyForUserViewModel>();
        }
        public IList<SimplePolicyForUserViewModel> Policies { get; set; }
    }
    public class PolicyForUserViewModel : SimplePolicyForUserViewModel {
        [ScriptIgnore]
        public Models.PolicyTextInfoPart PolicyText { get; set; }
    }

    public class SimplePolicyForUserViewModel {
        public int AnswerId { get; set; }
        public int PolicyTextId { get; set; }
        public DateTime AnswerDate { get; set; }
        public bool OldAccepted { get; set; }
        public bool Accepted { get; set; }
    
    }
}