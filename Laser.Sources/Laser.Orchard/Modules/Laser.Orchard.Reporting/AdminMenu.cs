using Orchard.Localization;
using Orchard.Projections;
using Orchard.UI.Navigation;

namespace Laser.Orchard.Reporting {
    public class AdminMenu : INavigationProvider
    {
        public AdminMenu() {
            T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder)
        {
            builder.Add(menu => {
                menu.Caption(T("Charts & Reports"))
                    .Permission(Security.Permissions.ShowDataDashboard)
                    .Position("3.07");
                menu.Add(T("Query Report Definition"), "1.0", sub1 => sub1
                    .Action("Index", "Report", new { area = "Laser.Orchard.Reporting" })
                    .Permission(Permissions.ManageQueries));

                //var reports = _contentManager.GetContentTypeDefinitions().Where(x => x.Parts.Any(y => y.PartDefinition.Name == "DataReportViewerPart") && x.Settings.Any(z => z.Key == "Stereotype" && z.Value == "Widget") == false);
                //foreach (var report in reports) {
                //    menu.Add(sub4 => sub4.Caption(T(" - " + report.DisplayName))
                //        .Action("List", "Admin", new RouteValueDictionary {
                //                {"area", "Laser.Orchard.AdvancedSearch"},
                //                {"model.Id", report.Name}
                //        }));
                //}

                //var dashboardType = _contentManager.GetContentTypeDefinitions().Where(x => x.Name == "DataReportDashboard").FirstOrDefault();
                //menu.Add(T("Dashboard Definitions"), "3.0", sub3 => sub3
                //    .Action("List", "Admin",
                //        new RouteValueDictionary {
                //                {"area", "Laser.Orchard.AdvancedSearch"},
                //                {"model.Id", dashboardType.Name}
                //        })
                //    .Permission(OrchardCoreContents.DynamicPermissions.CreateDynamicPermission(
                //        OrchardCoreContents.DynamicPermissions.PermissionTemplates["EditOwnContent"],
                //        dashboardType)));

                menu.Add(sub51 => sub51.Caption(T("Charts"))
                    .Permission(Security.Permissions.ShowDataReports)
                    .Action("ShowReports", "Report", new { area = "Laser.Orchard.Reporting" })
                );
                menu.Add(sub52 => sub52.Caption(T("Dashboards"))
                    .Permission(Security.Permissions.ShowDataDashboard)
                    .Action("DashboardList", "Report", new { area = "Laser.Orchard.Reporting" })
                );
            });
        }
    }
}