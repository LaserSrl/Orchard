using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Laser.Orchard.StartupConfig.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;

namespace Laser.Orchard.StartupConfig.Drivers {
    public class FavoriteCulturePartDriver : ContentPartDriver<FavoriteCulturePart> {
        protected override string Prefix {
            get {
                return "FavoriteCulturePart";
            }
        }
        protected override DriverResult Editor(FavoriteCulturePart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }
        protected override DriverResult Editor(FavoriteCulturePart part, IUpdateModel updater, dynamic shapeHelper) {
            if (updater != null) {
                if (updater.TryUpdateModel(part, Prefix, null, null)) { 
                
                }
            }
            return ContentShape("Parts_FavoriteCulturePart_Edit",
                                () => shapeHelper.EditorTemplate(TemplateName: "Parts/FavoriteCulturePart_Edit",
                                    Model: part,
                                    Prefix: Prefix)
                                    .ListCulture(new List<string> { "ciao", "ciao2"})
                                    );

        }
        protected override void Importing(FavoriteCulturePart part, ImportContentContext context) {
            base.Importing(part, context);
        }
        protected override void Exporting(FavoriteCulturePart part, ExportContentContext context) {
            base.Exporting(part, context);
        }
    }
}