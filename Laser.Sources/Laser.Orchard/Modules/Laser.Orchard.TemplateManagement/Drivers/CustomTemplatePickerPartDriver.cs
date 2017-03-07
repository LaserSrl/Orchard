using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.TemplateManagement.Models;
using Laser.Orchard.TemplateManagement.Services;
using Laser.Orchard.TemplateManagement.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;

namespace Laser.Orchard.TemplateManagement.Drivers {
    public class CustomTemplatePickerPartDriver : ContentPartCloningDriver<CustomTemplatePickerPart> {
        private readonly IContentManager _contentManager;
        private readonly ITemplateService _templateService;

        public CustomTemplatePickerPartDriver(IContentManager contentManager, ITemplateService templateService) {
            _contentManager = contentManager;
            _templateService = templateService;
        }
        protected override string Prefix {
            get { return "CustomTemplatePicker"; }
        }

        protected override DriverResult Editor(CustomTemplatePickerPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(CustomTemplatePickerPart part, IUpdateModel updater, dynamic shapeHelper) {
            var vModel = new CustomTemplatePickerViewModel {
                TemplateIdSelected = part.SelectedTemplate != null ? part.SelectedTemplate.Id : (int?)null,
                TemplatesList = _templateService.GetTemplates()
            };
            if (updater != null) {
                if (updater.TryUpdateModel(vModel, Prefix, null, null)) {
                    part.SelectedTemplate = _contentManager.Get<TemplatePart>(vModel.TemplateIdSelected.Value);
                } 
            }
            return ContentShape("Parts_CustomTemplatePicker_Edit", () => shapeHelper.EditorTemplate(TemplateName: "Parts/CustomTemplatePicker_Edit", Model: vModel, Prefix: Prefix));
        }

        //TODO: Importing/Exporting 
        protected override void Importing(CustomTemplatePickerPart part, ImportContentContext context) {
        }

        protected override void Exporting(CustomTemplatePickerPart part, ExportContentContext context) {
        }

        protected override void Cloning(CustomTemplatePickerPart originalPart, CustomTemplatePickerPart clonePart, CloneContentContext context) {
            clonePart.SelectedTemplate = originalPart.SelectedTemplate;
        }
    }
}