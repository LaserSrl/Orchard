using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace Laser.Orchard.UserReactions.Models {
  
    public class UserReactionsPart : ContentPart<UserReactionsPartRecord> {

        public IEnumerable<UserReactionsSummaryRecord> Reactions {
            get {
                return Record.Reactions;
            }
        }

    }

    public class UserReactionsPartRecord : ContentPartRecord {

        public UserReactionsPartRecord() {
            List<UserReactionsSummaryRecord> Reactions = new List<UserReactionsSummaryRecord>();
        }
       public virtual IList<UserReactionsSummaryRecord> Reactions { get; set; }
    }

   
    
}