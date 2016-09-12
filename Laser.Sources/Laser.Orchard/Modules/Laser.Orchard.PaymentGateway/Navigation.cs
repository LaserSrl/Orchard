﻿using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.PaymentGateway {
    public class Navigation : INavigationProvider {
        private readonly IEnumerable<IPosService> _posServices;
        public Localizer T { get; set; }

        public Navigation(IEnumerable<IPosService> posServices) {
            _posServices = posServices;
            T = NullLocalizer.Instance;
        }
        public string MenuName {
            get { return "admin"; }
        }
        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(T("Settings"), menu => menu
                .Add(T("Payment Gateway"), "10.0", subMenu => {
                    //subMenu.LinkToFirstChild(true);
                    subMenu.Action("Index", "Admin", new { area = "Laser.Orchard.PaymentGateway" }).Permission(StandardPermissions.SiteOwner);
                    subMenu.Add(T("Info"), "10.0", item => item.Action("Index", "Admin", new { area = "Laser.Orchard.PaymentGateway" }).LocalNav());
                    foreach (var pos in _posServices) {
                        subMenu.Add(T(pos.GetPosName()), "10.0", item => item.Action("Index", "Admin", new { area = pos.GetType().Namespace.Replace(".Services", "") }).LocalNav());
                    }
                })
            );
        }
    }
}