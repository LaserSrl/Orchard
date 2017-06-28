using Orchard.ContentManagement;
using Orchard.Projections.Descriptors;
using Laser.Orchard.Reporting.Models;
using Laser.Orchard.Reporting.Providers;
using System.Collections.Generic;
using Orchard;

namespace Laser.Orchard.Reporting.Services {
    public interface IReportManager: IDependency
    {
        IEnumerable<TypeDescriptor<GroupByDescriptor>> DescribeGroupByFields();
        IEnumerable<AggregationResult> RunReport(ReportRecord report, IContent container);
        IEnumerable<AggregationResult> RunHqlReport(ReportRecord report, IContent container);
        int GetCount(ReportRecord report, IContent container);
        IEnumerable<ReportItem> GetReportListForCurrentUser(string titleFilter = "");
        IEnumerable<DataReportViewerPart> GetReports();
    }
}
