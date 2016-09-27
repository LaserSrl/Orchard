using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.IXMSD.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace Laser.Orchard.IXMSD.Drivers {
    public class IXMSDPartDriver : ContentPartDriver<IXMSDPart> {

        private readonly IOrchardServices _orchardServices;
        private const string TemplateName = "Parts/IXMSDPart";
        protected override string Prefix {
            get { return "Laser.Orchard.IXMSD"; }
        }
        public IXMSDPartDriver(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
        }


        protected override DriverResult Editor(IXMSDPart part, dynamic shapeHelper) {

            return ContentShape("Parts_IXMSDPart_Edit",
                () => {
                    return shapeHelper.EditorTemplate(
                  TemplateName: TemplateName,
                  Model: part,
                  Prefix: Prefix);
                });
            //  return Editor(part, null, shapeHelper);

        }



        protected override DriverResult Editor(IXMSDPart part, IUpdateModel updater, dynamic shapeHelper) {

            if (updater != null) {
                if (updater.TryUpdateModel(part, Prefix, null, null)) {
                }
            }

            return Editor(part, shapeHelper);
        }

        protected override void Importing(IXMSDPart part, ImportContentContext context) {
            var importedExternalMediaUrl = context.Attribute(part.PartDefinition.Name, "ExternalMediaUrl");
            if (importedExternalMediaUrl != null) {
                part.ExternalMediaUrl = importedExternalMediaUrl;
            }
        }

        protected override void Exporting(IXMSDPart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("ExternalMediaUrl", part.ExternalMediaUrl);
        }



    }
}