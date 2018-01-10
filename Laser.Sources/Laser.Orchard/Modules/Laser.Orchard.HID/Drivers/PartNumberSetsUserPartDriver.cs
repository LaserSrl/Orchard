using Laser.Orchard.HID.Models;
using Orchard.ContentManagement.Drivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.CompilerServices;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;

namespace Laser.Orchard.HID.Drivers {
    public class PartNumberSetsUserPartDriver : ContentPartDriver<PartNumberSetsUserPart> {

        public PartNumberSetsUserPartDriver() { }

        protected override string Prefix { get { return "PartNumberSetsUserPart"; } }

        protected override DriverResult Editor(PartNumberSetsUserPart part, dynamic shapeHelper) {
            // TODO
            return null;
        }

        protected override DriverResult Editor(PartNumberSetsUserPart part, IUpdateModel updater, dynamic shapeHelper) {
            // TODO
            return null;
        }

        protected override void Exporting(PartNumberSetsUserPart part, ExportContentContext context) {
            // TODO
            base.Exporting(part, context);
        }

        protected override void Importing(PartNumberSetsUserPart part, ImportContentContext context) {
            // TODO
            base.Importing(part, context);
        }
    }
}