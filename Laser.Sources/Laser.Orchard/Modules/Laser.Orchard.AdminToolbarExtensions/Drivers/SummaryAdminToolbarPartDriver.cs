using Laser.Orchard.AdminToolbarExtensions.Models;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.AdminToolbarExtensions.Drivers {
    [OrchardFeature("Laser.Orchard.AdminToolbarExtensions.SummaryAdminToolbar")]
    public class SummaryAdminToolbarPartDriver : ContentPartDriver<SummaryAdminToolbarPart> {


        protected override DriverResult Display(SummaryAdminToolbarPart part, string displayType, dynamic shapeHelper) {

            var barSettings = part.Settings.GetModel<SummaryAdminToolbarSettings>();
            if (displayType == "SummaryAdmin") {
                return ContentShape("Parts_SummaryAdminToolbarPart_SummaryAdmin",
                    () => shapeHelper.Parts_SummaryAdminToolbarPart_SummaryAdmin(Toolbar: barSettings));
            }
            return new DriverResult();//base.Display(part, displayType, shapeHelper);
        }
    }
}