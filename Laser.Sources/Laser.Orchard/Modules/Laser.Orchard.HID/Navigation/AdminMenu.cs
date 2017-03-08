using Laser.Orchard.HID.Extensions;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Navigation;

namespace Laser.Orchard.HID.Navigation {
    public class AdminMenu : INavigationProvider {

        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(T("Settings"), menu => menu
                .Add(T("HID"), "10.0", subMenu => subMenu
                    .Action("Index", "Admin", new { area = Constants.LocalArea })
                    .Permission(StandardPermissions.SiteOwner)
                    .Add(T("Settings"), "10.0", item => item
                        .Action("Index", "Admin", new { area = Constants.LocalArea })
                        .Permission(StandardPermissions.SiteOwner)
                        .LocalNav())
                ));
        }
    }
}