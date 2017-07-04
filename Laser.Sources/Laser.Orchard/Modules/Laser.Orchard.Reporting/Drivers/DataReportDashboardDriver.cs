using Laser.Orchard.Reporting.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Reporting.Drivers {
    public class DataReportDashboardDriver : ContentPartDriver<DataReportDashboardPart> {
        public Localizer T;
        public DataReportDashboardDriver() {
            T = NullLocalizer.Instance;
        }
        protected override DriverResult Display(DataReportDashboardPart part, string displayType, dynamic shapeHelper) {
            return null;
        }
        protected override DriverResult Editor(DataReportDashboardPart part, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(part, "DataReportDashboardPart", null, null);
            return this.Editor(part, shapeHelper);
        }
        protected override DriverResult Editor(DataReportDashboardPart part, dynamic shapeHelper) {
            return ContentShape("Parts_DataReportDashboard_Edit",
                    () => shapeHelper.EditorTemplate(
                        TemplateName: "Parts/DataReportDashboard",
                        Model: part,
                        Prefix: "DataReportDashboardPart"));
        }
    }
}