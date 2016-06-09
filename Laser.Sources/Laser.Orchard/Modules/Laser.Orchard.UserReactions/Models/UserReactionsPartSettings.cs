using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.UserReactions.Models {
    public class UserReactionsPartSettings 
    {
        public bool Filtering { get; set; }
        public List<UserReactionsSettingTypesRecord> TypeReactionsPartsSelected { get; set; }
    }
}