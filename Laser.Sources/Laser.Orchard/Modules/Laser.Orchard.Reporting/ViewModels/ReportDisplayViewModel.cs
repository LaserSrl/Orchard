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
        public ContentItem FilterContent { get; set; }
        public ContentItem ViewerContent { get; set; }
    }
}