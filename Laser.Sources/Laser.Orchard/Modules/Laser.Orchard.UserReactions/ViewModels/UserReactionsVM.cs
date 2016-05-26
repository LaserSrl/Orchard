using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.UserReactions.ViewModels {
    
    public class UserReactionsTotalRec 
    {
        public UserReactionsTotalRec() {
            UserReactionsTotalRecord = new List<UserReactionsVM>();
        }

        public List<UserReactionsVM> UserReactionsTotalRecord { get; set; }
    }

 public class UserReactionsVM {
        public int Id { get; set; }
        public string TypeName { get; set; }
        public int Quantity { get; set; }
        public int TypeId { get; set; }
    }


}