using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.MailCommunication.Models;
using Laser.Orchard.TemplateManagement.Models;
using Laser.Orchard.TemplateManagement.Services;
using Laser.Orchard.TemplateManagement.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;

namespace Laser.Orchard.MailCommunication.Drivers {
    public class MailCommunicationPartDriver : ContentPartDriver<MailCommunicationPart> {
        private readonly IContentManager _contentManager;
        private readonly IOrchardServices _orchardServices;
        private readonly ITemplateService _templateService;


        public MailCommunicationPartDriver(IContentManager contentManager, IOrchardServices orchardServices, ITemplateService templateService) {
            _contentManager = contentManager;
            _orchardServices = orchardServices;
            _templateService = templateService;
        }
        protected override string Prefix {
            get { return "MailCommunicationPart"; }
        }

        protected override DriverResult Editor(MailCommunicationPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(MailCommunicationPart part, IUpdateModel updater, dynamic shapeHelper) {
            var vModel = new CustomTemplatePickerViewModel {
                TemplateIdSelected = part.ContentItem.As<CustomTemplatePickerPart>().SelectedTemplate != null ? part.ContentItem.As<CustomTemplatePickerPart>().SelectedTemplate.Id : (int?)null,
                TemplatesList = _templateService.GetTemplates().Where(w => ((dynamic)w.ContentItem).CustomTemplate.ForEmailCommunication.Value == true)
            };

            if (updater != null) {
                if (updater.TryUpdateModel(part, Prefix, null, null) && updater.TryUpdateModel(vModel, Prefix, null, null)) {
                    part.ContentItem.As<CustomTemplatePickerPart>().SelectedTemplate = _contentManager.Get<TemplatePart>(vModel.TemplateIdSelected.Value);
                    if (_orchardServices.WorkContext.HttpContext.Request.Form["submit.Save"] == "submit.MailTest") {
                        // Logica di invio mail forse meglio metterla in un handler > OnUpdated
                    }
                }
            }
            var shapes = new List<DriverResult>();
            shapes.Add(ContentShape("Parts_MailCommunication_Edit", () => shapeHelper.EditorTemplate(TemplateName: "Parts/MailCommunication_Edit", Model: part, Prefix: Prefix)));
            shapes.Add(ContentShape("Parts_MailCommunicationActions_Edit", () => shapeHelper.EditorTemplate(TemplateName: "Parts/MailCommunicationActions_Edit", Model: part, Prefix: Prefix)));
            shapes.Add(ContentShape("Parts_CustomTemplatePickerOverride_Edit", () => shapeHelper.EditorTemplate(TemplateName: "Parts/CustomTemplatePickerOverride_Edit", Model: vModel, Prefix: Prefix)));
            return new CombinedResult(shapes);
        }

        //TODO: Importing/Exporting 
        protected override void Importing(MailCommunicationPart part, ImportContentContext context) {
        }

        protected override void Exporting(MailCommunicationPart part, ExportContentContext context) {
        }
    }
}