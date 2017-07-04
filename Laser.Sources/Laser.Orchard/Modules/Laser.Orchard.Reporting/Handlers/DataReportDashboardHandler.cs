using Laser.Orchard.Reporting.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Reporting.Handlers {
    public class DataReportDashboardHandler : ContentHandler {
        public DataReportDashboardHandler(IRepository<DataReportDashboardPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}