using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Laser.Orchard.StartupConfig.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard;
using OrchardData = Orchard.Data;
using OrchardLocalization = Orchard.Localization;
using Orchard.UI.Admin;
using Orchard.Localization.Records;
using Orchard.Localization.Models;

namespace Laser.Orchard.StartupConfig.Drivers {
    public class FavoriteCulturePartDriver : ContentPartDriver<FavoriteCulturePart> {
        private readonly IOrchardServices _orchardServices;
        private readonly IContentManager _contentManager;

        protected override string Prefix {
            get {
                return "FavoriteCulturePart";
            }
        }
        public FavoriteCulturePartDriver(IOrchardServices orchardServices, IContentManager contentManager) {
            _orchardServices = orchardServices;
            _contentManager = contentManager;
        }
        protected override DriverResult Display(FavoriteCulturePart part, string displayType, dynamic shapeHelper) {
            bool isAdmin = AdminFilter.IsApplied(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
            if (isAdmin && (displayType == "Detail")) {
                string culture = "";
                OrchardData.IRepository<OrchardLocalization.Records.CultureRecord> cultureRepository;
                if (_orchardServices.WorkContext.TryResolve<OrchardData.IRepository<OrchardLocalization.Records.CultureRecord>>(out cultureRepository)) {
                    var cultureRecord = cultureRepository.Get(part.Culture_Id);
                    if (cultureRecord != null) {
                        culture = cultureRecord.Culture;
                    }
                }
                return ContentShape("Parts_FavoriteCulturePart",
                    () => shapeHelper.Parts_FavoriteCulturePart(Culture: culture));
            }
            else {
                return null;
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
            // mod 30-11-2016
            context.ImportAttribute(part.PartDefinition.Name, "Culture_Id", x => {
                var tempPartFromid = context.GetItemFromSession(x);

                if (tempPartFromid != null && tempPartFromid.Is<LocalizationPart>()) {
                    //associa id culture
                    part.Culture_Id = tempPartFromid.As<LocalizationPart>().Culture.Id;
                }
            });
        }

        protected override void Exporting(FavoriteCulturePart part, ExportContentContext context) {
            // mod 30-11-2016
            var root = context.Element(part.PartDefinition.Name);

            if (part.Culture_Id > 0) {
                //cerco il corrispondente valore dell' identity dalla parts del menu e lo associo al campo menuid 
                var contItemMenu = _contentManager.Get(part.Culture_Id);
                if (contItemMenu != null) {
                    root.SetAttributeValue("Culture_Id", _contentManager.GetItemMetadata(contItemMenu).Identity.ToString());
                }

            }

        }
    }
}