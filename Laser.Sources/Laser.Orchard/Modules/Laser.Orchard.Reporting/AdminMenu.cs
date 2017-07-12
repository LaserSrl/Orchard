using Laser.Orchard.Reporting.Models;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Projections;
using Orchard.UI.Navigation;
using System.Collections.Generic;

namespace Laser.Orchard.Reporting {
    public class AdminMenu : INavigationProvider
    {
        private readonly IContentManager _contentManager;
        private readonly ITransactionManager _transactionManager;
        public AdminMenu(IContentManager contentManager, ITransactionManager transactionManager) {
            _contentManager = contentManager;
            _transactionManager = transactionManager;
            T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder)
        {
            builder.AddImageSet("Data Reporting").Add(T("Data Reports"), "3",
                menu =>
                    menu.Add(T("Data Reports"), "1.0",
                    q => q.Action("Index", "Report", new { area = "Laser.Orchard.Reporting" }).Permission(Permissions.ManageQueries).LocalNav()), null);

            builder.Add(item => {
                item.Caption(T("Execute Data Reports"))
                    .Permission(Security.Permissions.ShowDataReports)
                    .Position("3.05")
                    .Action("ShowReports", "Report", new { area = "Laser.Orchard.Reporting" });
            });
            builder.Add(item => {
                item.Caption(T("Data Report Dashboards"))
                    .Permission(Security.Permissions.ShowDataDashboard)
                    .Position("3.06");
                var dashboardPermissions = new Security.Permissions(_contentManager, _transactionManager).GetDashboardPermissions();
                foreach(var dashboard in GetDashboards()) {
                    item.Add(T(dashboard.As<TitlePart>().Title), sub => sub
                        .Action("ShowDashboard", "Report", new { area = "Laser.Orchard.Reporting", Id = dashboard.Id })
                        .Permission(dashboardPermissions[dashboard.Id])
                    );
                }
            });
        }
        private IEnumerable<ContentItem> GetDashboards() {
            return _contentManager.Query().ForType("DataReportDashboard").List();
        }
    }
}