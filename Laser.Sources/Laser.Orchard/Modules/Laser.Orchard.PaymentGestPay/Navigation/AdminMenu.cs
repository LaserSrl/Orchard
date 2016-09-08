using Laser.Orchard.PaymentGestPay.Extensions;
using Orchard.Localization;
using Orchard.UI.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.PaymentGestPay.Navigation {
    public class AdminMenu : INavigationProvider {

        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(T("Settings"), menu => menu
                .Add(T("Payments"), "10.0", subMenu => subMenu
                    .Action("Index", "Admin", new { area = Constants.LocalArea })
                    .LinkToFirstChild(true)
                    .Add(new LocalizedString(Constants.PosName), "10.0", item => item
                        .Action("Index", "Admin", new { area = Constants.LocalArea })
                        .LocalNav()
                    )
                )
            );
        }
    }
}