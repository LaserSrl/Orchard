using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Localization;
using Orchard.UI.Navigation;
using Orchard.Core.Settings;

namespace Laser.Orchard.DevTools {
    public class Navigation : INavigationProvider {
        public string MenuName {
            get { return "admin"; }
        }

        public Navigation() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(item => item
                .Caption(T("DevTools"))
                .Position("1.01")
                .Action("Index", "Admin", new { area = "Laser.Orchard.DevTools" })
                .Permission(Permissions.DevTools)
       );
        }
    }
}


