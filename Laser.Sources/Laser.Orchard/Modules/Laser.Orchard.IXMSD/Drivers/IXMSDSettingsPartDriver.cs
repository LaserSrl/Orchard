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
    public class IXMSDSettingsPartDriver : ContentPartDriver<IXMSDSettingsPart> {
        private readonly IOrchardServices _orchardServices;
        private const string TemplateName = "Parts/IXMSDSettings";
        protected override string Prefix {
            get { return "Laser.Orchard.IXMSD"; }
        }
        public IXMSDSettingsPartDriver(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
        }
    

        protected override DriverResult Editor(IXMSDSettingsPart part, dynamic shapeHelper) {
    
            return ContentShape("Parts_IXMSDSettings_Edit",
                () => {
                    var getpart = _orchardServices.WorkContext.CurrentSite.As<IXMSDSettingsPart>();
                    return shapeHelper.EditorTemplate(
                    TemplateName: TemplateName,
                    Model: getpart,
                    Prefix: Prefix);
                }).OnGroup("IXMSD");
          //  return Editor(part, null, shapeHelper);

        }



        protected override DriverResult Editor(IXMSDSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
        
            return ContentShape("Parts_IXMSDSettings_Edit", () => {

                   if (updater != null) {
                if (updater.TryUpdateModel(part, Prefix, null, null)) {
                }
            }
                
                return shapeHelper.EditorTemplate(TemplateName: "Parts/PushMobileSettings_Edit", Model: part, Prefix: Prefix);
            })
                .OnGroup("IXMSD");
        }

        protected override void Importing(IXMSDSettingsPart part, ImportContentContext context) {
            //           context.ImportAttribute(part.PartDefinition.Name, "DefaultParserEngine", x => part.DefaultParserIdSelected = x);
        }

        protected override void Exporting(IXMSDSettingsPart part, ExportContentContext context) {
            //           context.Element(part.PartDefinition.Name).SetAttributeValue("DefaultParserEngine", part.DefaultParserIdSelected);


        }



    }
}