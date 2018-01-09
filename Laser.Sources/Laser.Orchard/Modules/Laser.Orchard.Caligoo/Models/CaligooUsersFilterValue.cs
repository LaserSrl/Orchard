using Laser.Orchard.Caligoo.Utils;
using System;
using System.Collections.Generic;

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
            if (WifiSessionDurationMin.HasValue) {
                pars.Add(string.Format("wifi_session_duration_min={0}", WifiSessionDurationMin));
            }
            if (WifiSessionDurationMax.HasValue) {
                pars.Add(string.Format("wifi_session_duration_max={0}", WifiSessionDurationMax));
            }
            if (VisitDurationMin.HasValue) {
                pars.Add(string.Format("visit_duration_min={0}", VisitDurationMin));
            }
            if (VisitDurationMax.HasValue) {
                pars.Add(string.Format("visit_duration_max={0}", VisitDurationMax));
            }
            if (string.IsNullOrWhiteSpace(LocationList) == false) {
                var list = LocationList.Trim().Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                foreach(var id in list) {
                    pars.Add(string.Format("location_list={0}", id.Trim()));
                }
            }
            if(Page > 0) {
                pars.Add(string.Format("page={0}", Page));
            }
            return string.Join("&", pars);
        }
    }
}