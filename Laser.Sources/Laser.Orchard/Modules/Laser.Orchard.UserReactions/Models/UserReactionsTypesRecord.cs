using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.UserReactions.Models {
    public class UserReactionsTypesRecord {
        public virtual int Id { get; set; }
        public virtual string TypeName { get; set; }
        public virtual string TypeCssClass { get; set; }
        public virtual int Priority { get; set; }
    }
}