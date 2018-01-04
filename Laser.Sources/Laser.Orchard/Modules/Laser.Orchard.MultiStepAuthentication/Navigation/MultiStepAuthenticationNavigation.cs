using Laser.Orchard.MultiStepAuthentication.Permissions;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.UI.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.MultiStepAuthentication.Navigation {
    [OrchardFeature("Laser.Orchard.MultiStepAuthentication")]
    public class MultiStepAuthenticationNavigation : INavigationProvider {

        public MultiStepAuthenticationNavigation() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public string MenuName {
            get { return "admin"; }
        }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(T("Settings"), menu => menu
                .Add(T("Multi-Step Authentication"), "10.0", submenu => {
                    submenu
                        .Permission(MultiStepAuthenticationPermissions.ConfigureAuthentication)
                        .LinkToFirstChild(true);
                }));
        }
    }
}