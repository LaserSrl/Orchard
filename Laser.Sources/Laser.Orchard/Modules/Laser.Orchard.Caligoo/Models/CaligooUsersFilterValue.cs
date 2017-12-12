using Laser.Orchard.Caligoo.Utils;
using System;

namespace Laser.Orchard.Caligoo.Models {
    public class CaligooUsersFilterValue {
        public string Type { get; set; }
        public bool Online { get; set; }
        public DateTime Since { get; set; }
        public DateTime Until { get; set; }
        public int WifiSessionDurationMin { get; set; }
        public int WifiSessionDurationMax { get; set; }
        public int VisitDurationMin { get; set; }
        public int VisitDurationMax { get; set; }
        public string LocationList { get; set; }
        public int Page { get; set; }
        public int SinceTimestamp {
            get {
                return new CaligooUtils().ConvertToTimestamp(Since);
            }
        }
        public int UntilTimestamp {
            get {
                return new CaligooUtils().ConvertToTimestamp(Until);
            }
        }
    }
}