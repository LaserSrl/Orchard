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
        }
    }
}