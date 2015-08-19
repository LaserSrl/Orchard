using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.WebServices {
    public enum SourceTypes { 
        ContentItem,
        Shape
    }

    public enum ResultTarget {
        Contents,
        Terms,
        SubTerms
    }

}