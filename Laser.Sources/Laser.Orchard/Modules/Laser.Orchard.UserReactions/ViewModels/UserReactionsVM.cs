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
        public string CssStyleName { get; set; }
        public int OrderPriority { get; set; }
        public bool Activating { get; set; }
        public int Clicked { get; set; }
    }


}