using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Mobile.ViewModels {
    [OrchardFeature("Laser.Orchard.PushGateway")]
    public class PushAndroidVM {
        public string Text { get; set; }
        public int Rid { get; set; }
        public int Id { get; set; }
        public string Ct { get; set; }
        public string Al { get; set; }
    }
}