using Laser.Orchard.Caligoo.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Laser.Orchard.Caligoo.Models {
    public class CaligooUsersFilterValue {
        public string Type { get; set; }
        public bool? Online { get; set; }
        public DateTime? Since { get; set; }
        public DateTime? Until { get; set; }
        public int? WifiSessionDurationMin { get; set; }
        public int? WifiSessionDurationMax { get; set; }
        public int? VisitDurationMin { get; set; }
        public int? VisitDurationMax { get; set; }
        public string LocationList { get; set; }
        public int Page { get; set; }
        public int? SinceTimestamp {
            get {
                if (Since.HasValue) {
                    return new CaligooUtils().ConvertToTimestamp(Since.Value);
                } else {
                    return null;
                }
            }
        }
        public int? UntilTimestamp {
            get {
                if (Until.HasValue) {
                    return new CaligooUtils().ConvertToTimestamp(Until.Value);
                } else {
                    return null;
                }
            }
        }
        public string GetQueryString() {
            var pars = new List<string>();
            if(string.IsNullOrWhiteSpace(Type) == false) {
                pars.Add(string.Format("type={0}", Type.Trim()));
            }
            if (Online.HasValue) {
                pars.Add(string.Format("online={0}", Online.Value));
            }
            if(SinceTimestamp.HasValue) {
                pars.Add(string.Format("since={0}", SinceTimestamp.Value));
            }
            if (UntilTimestamp.HasValue) {
                pars.Add(string.Format("until={0}", UntilTimestamp.Value));
            }

            return string.Join("&", pars);
        }
    }
}