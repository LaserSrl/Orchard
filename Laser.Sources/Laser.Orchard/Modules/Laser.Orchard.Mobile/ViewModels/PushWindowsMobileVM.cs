using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Mobile.ViewModels {
    [OrchardFeature("Laser.Orchard.PushGateway")]
    public class PushWindowsMobileVM {
        public string Title { get; set; }
        public string Text { get; set; }
        public int idRelated { get; set; }
        public int idContent { get; set; }
    }
}