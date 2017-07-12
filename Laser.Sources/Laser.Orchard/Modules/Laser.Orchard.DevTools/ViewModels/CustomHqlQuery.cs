using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.DevTools.ViewModels {
    public class CustomHqlQuery {
        public string HqlQuery { get; set; }
        public IEnumerable Results { get; set; }

        public CustomHqlQuery() {
            Results = new List<object>();
        }
    }
}