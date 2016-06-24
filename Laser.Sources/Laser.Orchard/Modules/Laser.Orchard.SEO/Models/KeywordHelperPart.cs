using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.SEO.Models {
    [OrchardFeature("Laser.Orchard.KeywordHelper")]
    public class KeywordHelperPart : ContentPart<KeywordHelperPartVersionRecord> {
        //this is a single string that will contain a comma separated list of keywords
        public string Keywords {
            get { return this.Retrieve(x => x.Keywords); }
            set { this.Store(x => x.Keywords, value); }
        }
    }
}