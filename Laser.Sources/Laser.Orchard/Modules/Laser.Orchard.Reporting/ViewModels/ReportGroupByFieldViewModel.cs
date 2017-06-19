using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Reporting.ViewModels
{
    public class ReportGroupByFieldViewModel
    {
        public LocalizedString Name { get; set; }
        public LocalizedString Description { get; set; }
        public string CategoryAndType { get; set; }
    }
}