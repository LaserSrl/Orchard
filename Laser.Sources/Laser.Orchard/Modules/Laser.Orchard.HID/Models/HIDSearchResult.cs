using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.HID.Models {
    public class HIDSearchResult {
        public int TotalResults { get; private set; }
        public int ItemsPerPage { get; private set; }
        public int StartIndex { get; private set; }
        public SearchErrors Error { get; set; }

        public HIDSearchResult() {
            Error = SearchErrors.UnknownError;
        }
        public HIDSearchResult(JObject result) {
            TotalResults = int.Parse(result["totalResults"].ToString());
            ItemsPerPage = int.Parse(result["itemsPerPage"].ToString());
            StartIndex = int.Parse(result["startIndex"].ToString());
            Error = SearchErrors.UnknownError;
        }
    }

    public class HIDUserSearchResult : HIDSearchResult {
        public HIDUser User { get; set; }

        public HIDUserSearchResult() : base() { }
        public HIDUserSearchResult(JObject result) : base(result) { }
    }
}