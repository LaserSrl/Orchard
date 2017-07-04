using Laser.Orchard.Reporting.Models;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.Localization;
using Orchard.Projections;
using Orchard.UI.Navigation;
using System.Collections.Generic;

namespace Laser.Orchard.Reporting {
    public class AdminMenu : INavigationProvider
    {
        private readonly IContentManager _contentManager;
        public AdminMenu(IContentManager contentManager) {
            _contentManager = contentManager;
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
                item.Caption(T("Admin Data Reports"))
                    .Permission(Security.Permissions.ShowDataReports)
                    .Position("3.05")
                    .Action("ShowReports", "Report", new { area = "Laser.Orchard.Reporting" });
            });
            builder.Add(item => {
                item.Caption(T("Data Report Dashboards"))
                    .Permission(Security.Permissions.ShowDataReports)
                    .Position("3.06");
                foreach(var dashboard in GetDashboards()) {
                    item.Add(T(dashboard.As<TitlePart>().Title), sub => sub
                        .Action("ShowDashboard", "Report", new { area = "Laser.Orchard.Reporting", Id = dashboard.Id })
                    );
                }
            });
        }
        private IEnumerable<ContentItem> GetDashboards() {
            return _contentManager.Query().ForType("DataReportDashboard").List();
        }
    }
}