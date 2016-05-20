using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Laser.Orchard.Policy.Models;

namespace Laser.Orchard.Policy.Models {
    public class UserPolicyAnswersRecord {
        public virtual int Id { get; set; }
        public virtual UserPolicyPartRecord UserPolicyPartRecord { get; set; }
        public virtual PolicyTextInfoPartRecord PolicyTextInfoPartRecord { get; set; }
        public virtual DateTime AnswerDate { get; set; }
        public virtual bool Accepted { get; set; }
    }

    public class UserPolicyAnswersHistoryRecord {
        public virtual int Id { get; set; }
        public virtual UserPolicyPartRecord UserPolicyPartRecord { get; set; }
        public virtual PolicyTextInfoPartRecord PolicyTextInfoPartRecord { get; set; }
        public virtual DateTime AnswerDate { get; set; }
        public virtual DateTime EndValidity { get; set; }
        public virtual bool Accepted { get; set; }
    }
}
