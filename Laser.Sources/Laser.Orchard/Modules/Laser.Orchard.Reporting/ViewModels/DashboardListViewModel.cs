using Laser.Orchard.Reporting.Models;
using Orchard.ContentManagement;
using Orchard.UI.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Reporting.ViewModels {
    public class DashboardListViewModel {
        public IEnumerable<GenericItem> Dashboards { get; set; }
        public string TitleFilter { get; set; }
        public dynamic Pager { get; set; }
        public PagerParameters PagerParameters { get; set; }
        public int? page { get; set; }
        public DashboardListViewModel() {
            PagerParameters = new PagerParameters();
        }
    }
}