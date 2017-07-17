using Laser.Orchard.Reporting.Models;
using Orchard.UI.Navigation;
using System.Collections.Generic;

namespace Laser.Orchard.Reporting.ViewModels {
    public class ShowReportsViewModel {
        public IEnumerable<ReportItem> Reports { get; set; }
        public string TitleFilter { get; set; }
        public dynamic Pager { get; set; }
        public PagerParameters PagerParameters { get; set; }
        public int? page { get; set; }
        public ShowReportsViewModel() {
            PagerParameters = new PagerParameters();
        }
    }
}