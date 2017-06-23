using Laser.Orchard.Reporting.Models;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Reporting.ViewModels {
    public class ReportDisplayViewModel {
        public int Id { get; set; }
        public ContentItem DataReportViewerContent { get; set; }
        //public List<string> InputParameters { get; set; }
        //public DataReportViewerPart ViewerPart { get; set; }
        //public List<ContentField> Filters { get; set; }
        //public ContentPart FilterPart { get; set; }
        //public dynamic FilterShape { get; set; }
        public ContentItem FilterContent { get; set; }
        //public ReportDisplayViewModel() {
        //    InputParameters = new List<string>();
        //    Filters = new List<ContentField>();
        //}
    }
}