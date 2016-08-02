using Laser.Orchard.UserReactions.Settings;
using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace Laser.Orchard.UserReactions.Fields {
    public class UserReactionsField : ContentField {
        public string Value {
            get { return Storage.Get<string>(); }
            set { Storage.Set(value); }
        }

    }
}