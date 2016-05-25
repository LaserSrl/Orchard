using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.UserReactions.Models {
    public class UserReactionsSummaryRecord {


        public virtual int Id { get; set; }
        public virtual int Quantity { get; set; }

        public virtual UserReactionsPartRecord UserReaction { get; set; }
        public virtual UserReactionsTypesRecord UserReactionType { get; set; }

    }
}