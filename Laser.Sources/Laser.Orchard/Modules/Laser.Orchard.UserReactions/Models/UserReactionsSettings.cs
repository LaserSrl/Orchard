using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;

namespace Laser.Orchard.UserReactions.Models {

    public class UserReactionsSettingsPart : ContentPart {

        public StyleFileNameProviders StyleFileNameProvider {
            get { return this.Retrieve(x => x.StyleFileNameProvider); }
            set { this.Store(x => x.StyleFileNameProvider, value); }
        }

    }

    //public class UserReactionsSettingsPart : ContentPart {

    //    public UserReactionsSettingsPart() {
    //        List<string> StylesFiles = new List<string>();
    //        StylesFiles.Add("Reactions");
    //        StylesFiles.Add("ColoredReactions");

    //    }
    //    public virtual IList<string> StylesFiles { get; set; }
    //}
}