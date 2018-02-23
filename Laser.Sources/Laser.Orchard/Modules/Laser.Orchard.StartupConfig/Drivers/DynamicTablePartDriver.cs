using Laser.Orchard.StartupConfig.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.Drivers {
    public class DynamicTablePartDriver : ContentPartDriver<DynamicTablePart> {
        protected override DriverResult Editor(
        DynamicTablePart part, dynamic shapeHelper) {
            return ContentShape("Parts_DynamicTable_Edit",
            () => shapeHelper.EditorTemplate(
            TemplateName: "Parts/DynamicTable",
            Model: part,
            Prefix: Prefix));
        }
        protected override DriverResult Editor(
        DynamicTablePart part, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }
    }
}