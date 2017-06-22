using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Reporting.ViewModels {
    public class ReportDisplayViewModel {
        public int Id { get; set; }
        public ContentItem DataReportViewerContent { get; set; }
        public List<string> InputParameters { get; set; }
        public ReportDisplayViewModel() {
            InputParameters = new List<string>();
        }
    }
}